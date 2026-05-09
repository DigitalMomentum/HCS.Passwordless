using Umbraco.Cms.Core.Security;

namespace HCS.Passwordless.Services;

/// <summary>Resolves Umbraco members by email address or WebAuthn user-handle.</summary>
public interface IMemberLookupService
{
    /// <summary>Finds an approved member by <paramref name="email"/>. Returns <c>null</c> when not found or not approved.</summary>
    Task<MemberIdentityUser?> FindApprovedAsync(string email, CancellationToken ct = default);

    /// <summary>Finds an approved member by their WebAuthn <paramref name="userHandle"/>. Returns <c>null</c> when not found or not approved.</summary>
    Task<MemberIdentityUser?> FindApprovedByUserHandleAsync(byte[] userHandle, CancellationToken ct = default);
}
