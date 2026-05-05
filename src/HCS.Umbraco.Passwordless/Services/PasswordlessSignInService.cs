using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Web.Common.Security;

namespace HCS.Umbraco.Passwordless.Services;

internal sealed class PasswordlessSignInService : IPasswordlessSignInService
{
    private readonly UserManager<MemberIdentityUser> _userManager;
    private readonly IMemberSignInManager _signInManager;
    private readonly ILogger<PasswordlessSignInService> _logger;

    public PasswordlessSignInService(
        UserManager<MemberIdentityUser> userManager,
        IMemberSignInManager signInManager,
        ILogger<PasswordlessSignInService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    public async Task SignInAndRotateAsync(
        MemberIdentityUser member,
        bool isPersistent = true,
        string authenticationMethod = "passwordless",
        CancellationToken ct = default)
    {
        _logger.LogDebug("SignInAndRotate: updating security stamp for memberId={MemberId}", member.Id);
        var stampResult = await _userManager.UpdateSecurityStampAsync(member);
        _logger.LogDebug("SignInAndRotate: UpdateSecurityStampAsync succeeded={Succeeded} errors={Errors}",
            stampResult.Succeeded,
            stampResult.Succeeded ? "(none)" : string.Join(", ", stampResult.Errors.Select(e => e.Description)));

        member = (await _userManager.FindByIdAsync(member.Id))!;
        _logger.LogDebug("SignInAndRotate: refreshed member — id={MemberId} newStamp={Stamp}", member?.Id, member?.SecurityStamp);

        if (member is null)
        {
            _logger.LogDebug("SignInAndRotate: FindByIdAsync returned null — aborting sign-in");
            return;
        }

        _logger.LogDebug("SignInAndRotate: calling SignInAsync method={Method} isPersistent={IsPersistent}", authenticationMethod, isPersistent);
        await _signInManager.SignInAsync(member, isPersistent, authenticationMethod);
        _logger.LogDebug("SignInAndRotate: SignInAsync returned");
    }
}
