using Umbraco.Cms.Core.Security;

namespace HCS.Passwordless.Services;

/// <summary>Signs in an Umbraco member after successful passwordless verification.</summary>
public interface IPasswordlessSignInService
{
    /// <summary>
    /// Signs in <paramref name="member"/> and issues a new authentication cookie.
    /// The security stamp is rotated so any previous sessions are invalidated.
    /// </summary>
    Task SignInAndRotateAsync(
        MemberIdentityUser member,
        bool isPersistent = true,
        string authenticationMethod = "passwordless",
        CancellationToken ct = default);
}
