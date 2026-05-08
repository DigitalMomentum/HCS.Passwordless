using Umbraco.Cms.Core.Security;

namespace HCS.Umbraco.Passwordless.Services;

public interface IPasswordlessSignInService
{
    Task SignInAndRotateAsync(
        MemberIdentityUser member,
        bool isPersistent = true,
        string authenticationMethod = "passwordless",
        CancellationToken ct = default);
}
