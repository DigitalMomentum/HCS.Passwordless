using Umbraco.Cms.Core.Security;

namespace HCS.Umbraco.Passwordless.Services;

public interface IMemberLookupService
{
    Task<MemberIdentityUser?> FindApprovedAsync(string email, CancellationToken ct = default);
    Task<MemberIdentityUser?> FindApprovedByUserHandleAsync(byte[] userHandle, CancellationToken ct = default);
}
