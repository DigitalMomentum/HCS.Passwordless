namespace HCS.Umbraco.Passwordless.WebAuthn.Storage;

public interface IMemberCredentialStore
{
    Task<IReadOnlyList<StoredCredential>> GetByMemberAsync(Guid memberKey, CancellationToken ct = default);
    Task<StoredCredential?> GetByCredentialIdAsync(byte[] credentialId, CancellationToken ct = default);
    Task<StoredCredential> AddAsync(StoredCredential credential, CancellationToken ct = default);
    Task UpdateAfterAssertionAsync(byte[] credentialId, uint newCounter, DateTime lastUsedUtc, CancellationToken ct = default);
    Task<bool> RenameAsync(Guid memberKey, Guid credentialRowId, string nickname, CancellationToken ct = default);
    Task<bool> RemoveAsync(Guid memberKey, Guid credentialRowId, CancellationToken ct = default);
    Task<int> CountForMemberAsync(Guid memberKey, CancellationToken ct = default);
    Task RemoveAllForMemberAsync(Guid memberKey, CancellationToken ct = default);
}
