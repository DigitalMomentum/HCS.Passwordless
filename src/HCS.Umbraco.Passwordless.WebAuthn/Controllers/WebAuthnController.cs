using System.Security.Cryptography;
using System.Text;
using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using HCS.Umbraco.Passwordless.Configuration;
using HCS.Umbraco.Passwordless.Endpoints.Shared;
using HCS.Umbraco.Passwordless.RateLimiting;
using HCS.Umbraco.Passwordless.Services;
using HCS.Umbraco.Passwordless.WebAuthn.Configuration;
using HCS.Umbraco.Passwordless.WebAuthn.Dtos;
using HCS.Umbraco.Passwordless.WebAuthn.Notifications;
using HCS.Umbraco.Passwordless.WebAuthn.Services;
using HCS.Umbraco.Passwordless.WebAuthn.Storage;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Web.Common.Controllers;
using Microsoft.Extensions.Logging;

namespace HCS.Umbraco.Passwordless.WebAuthn.Controllers;

[ApiController]
[Route("auth/webauthn")]
public partial class WebAuthnController : UmbracoApiController
{
    private readonly IMemberManager _memberManager;
    private readonly IMemberLookupService _lookup;
    private readonly IMemberCredentialStore _store;
    private readonly IFido2 _fido2;
    private readonly IWebAuthnChallengeStore _challenges;
    private readonly IPasswordlessSignInService _signIn;
    private readonly IPasswordlessRateLimiter _limiter;
    private readonly IEventAggregator _events;
    private readonly IOptionsMonitor<PasswordlessOptions> _baseOpts;
    private readonly IOptionsMonitor<WebAuthnOptions> _waOpts;
    private readonly ILogger<WebAuthnController> _logger;


    public WebAuthnController(
        IMemberManager memberManager,
        IMemberLookupService lookup,
        IMemberCredentialStore store,
        IFido2 fido2,
        IWebAuthnChallengeStore challenges,
        IPasswordlessSignInService signIn,
        IPasswordlessRateLimiter limiter,
        IEventAggregator events,
        IOptionsMonitor<PasswordlessOptions> baseOpts,
        IOptionsMonitor<WebAuthnOptions> waOpts,
        ILogger<WebAuthnController> logger)
    {
        _memberManager = memberManager;
        _lookup = lookup;
        _store = store;
        _fido2 = fido2;
        _challenges = challenges;
        _signIn = signIn;
        _limiter = limiter;
        _events = events;
        _baseOpts = baseOpts;
        _waOpts = waOpts;
        _logger = logger;

    }

    [HttpPost("register/options")]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> RegisterOptions([FromBody] RegisterOptionsRequest dto, CancellationToken ct)
    {
        var waOpts = _waOpts.CurrentValue;
        if (!waOpts.Enabled) return NotFound();

        var member = await _memberManager.GetCurrentMemberAsync();
        if (member is null) return Forbid();

        var existing = await _store.GetByMemberAsync(member.Key, ct);
        var excludeList = existing.Select(c => new PublicKeyCredentialDescriptor(c.CredentialId)).ToList();

        var fido2User = new Fido2User
        {
            Id = member.Key.ToByteArray(),
            Name = member.Email ?? member.UserName ?? member.Id,
            DisplayName = member.Name ?? member.Email ?? member.UserName ?? member.Id
        };

        var authenticatorSelection = new AuthenticatorSelection
        {
            ResidentKey = waOpts.ResidentKey,
            UserVerification = waOpts.UserVerification,
            AuthenticatorAttachment = waOpts.AuthenticatorAttachment
        };

        var createOptions = _fido2.RequestNewCredential(
            new RequestNewCredentialParams
            {
                User = fido2User,
                ExcludeCredentials = excludeList,
                AuthenticatorSelection = authenticatorSelection,
                AttestationPreference = waOpts.AttestationPreference
            });

        var ceremonyId = $"pwl:webauthn:reg:{member.Key}:{Guid.NewGuid()}";
        var state = new RegistrationCeremonyState(createOptions, dto.Nickname, member.Key);
        await _challenges.PutAsync(ceremonyId, state, waOpts.ChallengeTtl, ct);

        return Ok(new { CeremonyId = ceremonyId, Options = createOptions });
    }

    [HttpPost("register/complete")]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> RegisterComplete([FromBody] RegisterCompleteRequest dto, CancellationToken ct)
    {
        if (!_waOpts.CurrentValue.Enabled) return NotFound();

        var member = await _memberManager.GetCurrentMemberAsync();
        if (member is null) return Forbid();

        var state = await _challenges.TakeAsync<RegistrationCeremonyState>(dto.CeremonyId, ct);
        if (state is null || state.MemberKey != member.Key)
            return BadRequest(new { error = "invalid_ceremony" });

        IsCredentialIdUniqueToUserAsyncDelegate isUnique = async (args, innerCt) =>
        {
            var found = await _store.GetByCredentialIdAsync(args.CredentialId, innerCt);
            return found is null;
        };

        RegisteredPublicKeyCredential result;
        try
        {
            result = await _fido2.MakeNewCredentialAsync(
                new MakeNewCredentialParams
                {
                    AttestationResponse = dto.Attestation,
                    OriginalOptions = state.Options,
                    IsCredentialIdUniqueToUserCallback = isUnique
                }, ct);
        }
        catch (Exception ex)
        {
            var refGuid = Guid.NewGuid();
            LogError(ex, refGuid);
            return BadRequest(new { error = "attestation_failed", detail = $"Reference: {refGuid}" });
        }

        var credential = new StoredCredential(
            Id: Guid.NewGuid(),
            MemberKey: member.Key,
            CredentialId: result.Id,
            PublicKey: result.PublicKey,
            UserHandle: member.Key.ToByteArray(),
            SignatureCounter: result.SignCount,
            CredType: "public-key",
            AaGuid: result.AaGuid,
            Transports: result.Transports is not null ? string.Join(",", result.Transports) : null,
            BackupEligible: result.IsBackupEligible,
            BackupState: result.IsBackedUp,
            Nickname: state.Nickname,
            CreatedUtc: DateTime.UtcNow,
            LastUsedUtc: null,
            AttestationFormat: result.AttestationFormat);

        var saved = await _store.AddAsync(credential, ct);

        return Ok(new CredentialView(
            saved.Id, saved.Nickname, null, saved.AaGuid,
            saved.CreatedUtc, saved.LastUsedUtc,
            saved.BackupEligible, saved.BackupState, saved.Transports));
    }

    [HttpPost("signin/options")]
    public async Task<IActionResult> SignInOptions([FromBody] SignInOptionsRequest dto, CancellationToken ct)
    {
        var waOpts = _waOpts.CurrentValue;
        if (!waOpts.Enabled) return NotFound();

        List<PublicKeyCredentialDescriptor> allowList;
        Guid? memberKey = null;
        bool isDecoy;

        if (string.IsNullOrWhiteSpace(dto.Email))
        {
            allowList = [];
            isDecoy = false;
        }
        else
        {
            var member = await _lookup.FindApprovedAsync(dto.Email, ct);
            if (member is not null)
            {
                var credentials = await _store.GetByMemberAsync(member.Key, ct);
                if (credentials.Count > 0)
                {
                    allowList = [.. credentials.Select(c => new PublicKeyCredentialDescriptor(c.CredentialId))];
                    memberKey = member.Key;
                    isDecoy = false;
                }
                else
                {
                    allowList = BuildDecoyAllowList(dto.Email);
                    isDecoy = true;
                }
            }
            else
            {
                allowList = BuildDecoyAllowList(dto.Email);
                isDecoy = true;
            }
        }

        var assertionOptions = _fido2.GetAssertionOptions(new GetAssertionOptionsParams
        {
            AllowedCredentials = allowList,
            UserVerification = waOpts.UserVerification
        });

        var ceremonyId = $"pwl:webauthn:sig:{Guid.NewGuid()}";
        var state = new AssertionCeremonyState(assertionOptions, memberKey, isDecoy);
        await _challenges.PutAsync(ceremonyId, state, waOpts.ChallengeTtl, ct);

        return Ok(new { CeremonyId = ceremonyId, Options = assertionOptions });
    }

    [HttpPost("signin/complete")]
    public async Task<IActionResult> SignInComplete([FromBody] SignInCompleteRequest dto, CancellationToken ct)
    {
        if (!_waOpts.CurrentValue.Enabled) return NotFound();

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        if (!await _limiter.TryAcquireAsync($"webauthn-signin-complete:ip:{ip}", TimeSpan.FromMinutes(1), 5, ct))
            return StatusCode(429);

        var state = await _challenges.TakeAsync<AssertionCeremonyState>(dto.CeremonyId, ct);
        if (state is null) return BadRequest(new { error = "invalid_ceremony" });

        if (state.IsDecoy)
        {
            await FakeWork.DelayAsync(TimeSpan.FromMilliseconds(120), ct);
            return Unauthorized();
        }

        var storedCredential = await _store.GetByCredentialIdAsync(dto.Assertion.RawId, ct);
        if (storedCredential is null) return Unauthorized();

        var member = state.MemberKey.HasValue
            ? await _lookup.FindApprovedAsync(state.MemberKey.Value.ToString(), ct)
            : await _lookup.FindApprovedByUserHandleAsync(dto.Assertion.Response.UserHandle ?? Array.Empty<byte>(), ct);

        if (member is null) return Unauthorized();
        if (state.MemberKey.HasValue && member.Key != state.MemberKey.Value) return Unauthorized();
        if (storedCredential.MemberKey != member.Key) return Unauthorized();

        VerifyAssertionResult result;
        try
        {
            result = await _fido2.MakeAssertionAsync(
                new MakeAssertionParams
                {
                    AssertionResponse = dto.Assertion,
                    OriginalOptions = state.Options,
                    StoredPublicKey = storedCredential.PublicKey,
                    StoredSignatureCounter = storedCredential.SignatureCounter,
                    IsUserHandleOwnerOfCredentialIdCallback = (args, _) =>
                        Task.FromResult(args.UserHandle.AsSpan().SequenceEqual(storedCredential.UserHandle))
                }, ct);
        }
        catch (Exception ex)
        {
            var refGuid = Guid.NewGuid();
            LogError(ex, refGuid);
            return BadRequest(new { error = "assertion_failed", detail = $"Reference: {refGuid}" });
        }

        if (result.SignCount != 0 && result.SignCount <= storedCredential.SignatureCounter)
        {
            await _events.PublishAsync(new PasskeyCounterRegressionNotification
            {
                MemberKey = storedCredential.MemberKey,
                CredentialId = storedCredential.CredentialId,
                StoredCounter = storedCredential.SignatureCounter,
                ReceivedCounter = result.SignCount
            });
            return Unauthorized();
        }

        await _store.UpdateAfterAssertionAsync(storedCredential.CredentialId, result.SignCount, DateTime.UtcNow, ct);
        await _signIn.SignInAndRotateAsync(member, isPersistent: true, authenticationMethod: "webauthn", ct: ct);

        return Ok(new { ok = true });
    }

    private static List<PublicKeyCredentialDescriptor> BuildDecoyAllowList(string email)
    {
        var seed = HMACSHA256.HashData(
            Encoding.UTF8.GetBytes("decoy-secret"),
            Encoding.UTF8.GetBytes(email.ToLowerInvariant()));
        return
        [
            new(seed[..32])
        ];
    }
}
