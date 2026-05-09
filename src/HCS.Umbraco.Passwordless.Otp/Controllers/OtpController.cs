using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using HCS.Umbraco.Passwordless.Configuration;
using HCS.Umbraco.Passwordless.Endpoints.Dtos;
using HCS.Umbraco.Passwordless.Endpoints.Shared;
using HCS.Umbraco.Passwordless.Otp.Configuration;
using HCS.Umbraco.Passwordless.Otp.Endpoints.Dtos;
using HCS.Umbraco.Passwordless.Otp.Notifications;
using HCS.Umbraco.Passwordless.Otp.Services;
using HCS.Umbraco.Passwordless.RateLimiting;
using HCS.Umbraco.Passwordless.Security.TokenProviders;
using HCS.Umbraco.Passwordless.Services;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Web.Common.Controllers;

namespace HCS.Umbraco.Passwordless.Otp.Controllers;

[ApiController]
[Route("auth/otp")]
public class OtpController : UmbracoApiController
{
    private readonly IMemberLookupService _lookup;
    private readonly UserManager<MemberIdentityUser> _users;
    private readonly IOtpNotificationSender _sender;
    private readonly IPasswordlessSignInService _signIn;
    private readonly IAttemptCounter _attempts;
    private readonly IPasswordlessRateLimiter _limiter;
    private readonly IOptionsMonitor<PasswordlessOptions> _baseOpts;
    private readonly IOptionsMonitor<OtpOptions> _otpOpts;
    private readonly ILoggerFactory _loggerFactory;

    public OtpController(
        IMemberLookupService lookup,
        UserManager<MemberIdentityUser> users,
        IOtpNotificationSender sender,
        IPasswordlessSignInService signIn,
        IAttemptCounter attempts,
        IPasswordlessRateLimiter limiter,
        IOptionsMonitor<PasswordlessOptions> baseOpts,
        IOptionsMonitor<OtpOptions> otpOpts,
        ILoggerFactory loggerFactory)
    {
        _lookup = lookup;
        _users = users;
        _sender = sender;
        _signIn = signIn;
        _attempts = attempts;
        _limiter = limiter;
        _baseOpts = baseOpts;
        _otpOpts = otpOpts;
        _loggerFactory = loggerFactory;
    }

    [HttpPost("request")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendOtp([FromBody] RequestByEmailDto dto, CancellationToken ct)
    {
        var baseOpts = _baseOpts.CurrentValue;
        var otpOpts = _otpOpts.CurrentValue;
        if (!otpOpts.Enabled) return NotFound();

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var normalizedEmail = (dto.Email ?? string.Empty).Trim().ToLowerInvariant();
        var emailHash = Sha256Helper.Hash(normalizedEmail);

        if (!await _limiter.TryAcquireAsync($"otp-req:ip:{ip}", TimeSpan.FromMinutes(1), baseOpts.RateLimits.PerIpRequestsPerMinute, ct)
            || !await _limiter.TryAcquireAsync($"otp-req:email:{emailHash}", TimeSpan.FromHours(1), baseOpts.RateLimits.PerEmailRequestsPerHour, ct))
            return StatusCode(429);

        var member = await _lookup.FindApprovedAsync(normalizedEmail, ct);
        if (member is not null)
        {
            var code = await _users.GenerateUserTokenAsync(member, TokenProviderNames.Otp, TokenProviderNames.PurposeOtpLogin);
            await _sender.SendOtpAsync(member, code, otpOpts.TokenLifespan, ct);
        }
        else
        {
            await FakeWork.DelayAsync(baseOpts.RateLimits.FakeWorkDelay, ct);
        }

        return Accepted(new { ok = true });
    }

    [HttpPost("verify")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Verify([FromBody] VerifyOtpDto dto, CancellationToken ct)
    {
        var baseOpts = _baseOpts.CurrentValue;
        var otpOpts = _otpOpts.CurrentValue;
        if (!otpOpts.Enabled) return NotFound();

        var logger = _loggerFactory.CreateLogger("HCS.Passwordless.Otp.Verify");
        logger.LogDebug("OTP verify: email={Email} returnUrl={ReturnUrl}", dto.Email, dto.ReturnUrl);

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        if (!await _limiter.TryAcquireAsync($"otp-verify:ip:{ip}", TimeSpan.FromMinutes(1), baseOpts.RateLimits.VerifyPerIpPerMinute, ct))
        {
            logger.LogDebug("OTP verify: rate-limited ip={Ip}", ip);
            return StatusCode(429);
        }

        var normalizedEmail = (dto.Email ?? string.Empty).Trim().ToLowerInvariant();
        var member = await _lookup.FindApprovedAsync(normalizedEmail, ct);
        if (member is null)
        {
            logger.LogDebug("OTP verify: member not found or not approved for email={Email}", normalizedEmail);
            await FakeWork.DelayAsync(baseOpts.RateLimits.FakeWorkDelay, ct);
            return Unauthorized();
        }

        logger.LogDebug("OTP verify: member found id={MemberId} stamp={Stamp}", member.Id, member.SecurityStamp);

        var (count, isLocked) = await _attempts.IncrementAndCheckAsync(
            member.Id, TokenProviderNames.PurposeOtpLogin,
            otpOpts.MaxAttempts, otpOpts.LockoutDuration, ct);

        logger.LogDebug("OTP verify: attempt count={Count} isLocked={IsLocked}", count, isLocked);
        if (isLocked) return StatusCode(429);

        var valid = await _users.VerifyUserTokenAsync(member, TokenProviderNames.Otp, TokenProviderNames.PurposeOtpLogin, dto.Code ?? string.Empty);
        logger.LogDebug("OTP verify: VerifyUserTokenAsync result={Valid} provider={Provider}", valid, TokenProviderNames.Otp);

        if (!valid)
        {
            await FakeWork.DelayAsync(baseOpts.RateLimits.FakeWorkDelay, ct);
            return Unauthorized();
        }

        await _attempts.ResetAsync(member.Id, TokenProviderNames.PurposeOtpLogin, ct);
        var safe = ReturnUrlValidator.Sanitize(dto.ReturnUrl, baseOpts.PostLoginRedirectPath);

        logger.LogDebug("OTP verify: code valid — calling SignInAndRotateAsync memberId={MemberId}", member.Id);
        await _signIn.SignInAndRotateAsync(member, isPersistent: true, authenticationMethod: "otp", ct: ct);
        logger.LogDebug("OTP verify: sign-in complete — returning success redirectTo={Safe}", safe);

        return Ok(new AuthResultDto(true, safe, null));
    }
}
