using NPoco;
using Umbraco.Cms.Infrastructure.Scoping;

namespace HCS.Passwordless.WebAuthn.Storage;

internal sealed class UmbracoDbMemberCredentialStore : IMemberCredentialStore
{
    private readonly IScopeProvider _scopeProvider;

    public UmbracoDbMemberCredentialStore(IScopeProvider scopeProvider)
        => _scopeProvider = scopeProvider;

    public async Task<IReadOnlyList<StoredCredential>> GetByMemberAsync(Guid memberKey, CancellationToken ct = default)
    {
        using var scope = _scopeProvider.CreateScope();
        var rows = await scope.Database.FetchAsync<MemberCredentialDto>(
            "WHERE MemberKey = @0", memberKey);
        scope.Complete();
        return rows.Select(ToRecord).ToList();
    }

    public async Task<StoredCredential?> GetByCredentialIdAsync(byte[] credentialId, CancellationToken ct = default)
    {
        using var scope = _scopeProvider.CreateScope();
        var row = await scope.Database.SingleOrDefaultAsync<MemberCredentialDto>(
            "WHERE CredentialId = @0", credentialId);
        scope.Complete();
        return row is null ? null : ToRecord(row);
    }

    public async Task<StoredCredential> AddAsync(StoredCredential credential, CancellationToken ct = default)
    {
        var dto = ToDto(credential);
        if (dto.Id == Guid.Empty) dto.Id = Guid.NewGuid();

        using var scope = _scopeProvider.CreateScope();
        await scope.Database.InsertAsync(dto);
        scope.Complete();
        return ToRecord(dto);
    }

    public async Task UpdateAfterAssertionAsync(byte[] credentialId, uint newCounter, DateTime lastUsedUtc, bool hasEverIncremented, CancellationToken ct = default)
    {
        using var scope = _scopeProvider.CreateScope();
        await scope.Database.ExecuteAsync(
            "UPDATE Passwordless_MemberCredentials SET SignatureCounter = @0, LastUsedUtc = @1, HasEverIncrementedCounter = @2 WHERE CredentialId = @3",
            (long)newCounter, lastUsedUtc, hasEverIncremented, credentialId);
        scope.Complete();
    }

    public async Task<bool> RenameAsync(Guid memberKey, Guid credentialRowId, string nickname, CancellationToken ct = default)
    {
        using var scope = _scopeProvider.CreateScope();
        var rows = await scope.Database.ExecuteAsync(
            "UPDATE Passwordless_MemberCredentials SET Nickname = @0 WHERE Id = @1 AND MemberKey = @2",
            nickname, credentialRowId, memberKey);
        scope.Complete();
        return rows > 0;
    }

    public async Task<bool> RemoveAsync(Guid memberKey, Guid credentialRowId, CancellationToken ct = default)
    {
        using var scope = _scopeProvider.CreateScope();
        var rows = await scope.Database.ExecuteAsync(
            "DELETE FROM Passwordless_MemberCredentials WHERE Id = @0 AND MemberKey = @1",
            credentialRowId, memberKey);
        scope.Complete();
        return rows > 0;
    }

    public async Task<int> CountForMemberAsync(Guid memberKey, CancellationToken ct = default)
    {
        using var scope = _scopeProvider.CreateScope();
        var count = await scope.Database.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Passwordless_MemberCredentials WHERE MemberKey = @0", memberKey);
        scope.Complete();
        return count;
    }

    public async Task RemoveAllForMemberAsync(Guid memberKey, CancellationToken ct = default)
    {
        using var scope = _scopeProvider.CreateScope();
        await scope.Database.ExecuteAsync(
            "DELETE FROM Passwordless_MemberCredentials WHERE MemberKey = @0", memberKey);
        scope.Complete();
    }

    private static StoredCredential ToRecord(MemberCredentialDto d) => new(
        d.Id, d.MemberKey, d.CredentialId, d.PublicKey, d.UserHandle,
        (uint)d.SignatureCounter, d.CredType, d.AaGuid, d.Transports,
        d.BackupEligible, d.BackupState, d.Nickname,
        d.CreatedUtc, d.LastUsedUtc, d.AttestationFormat,
        d.HasEverIncrementedCounter);

    private static MemberCredentialDto ToDto(StoredCredential c) => new()
    {
        Id = c.Id,
        MemberKey = c.MemberKey,
        CredentialId = c.CredentialId,
        PublicKey = c.PublicKey,
        UserHandle = c.UserHandle,
        SignatureCounter = (long)c.SignatureCounter,
        CredType = c.CredType,
        AaGuid = c.AaGuid,
        Transports = c.Transports,
        BackupEligible = c.BackupEligible,
        BackupState = c.BackupState,
        Nickname = c.Nickname,
        CreatedUtc = c.CreatedUtc,
        LastUsedUtc = c.LastUsedUtc,
        AttestationFormat = c.AttestationFormat,
        HasEverIncrementedCounter = c.HasEverIncrementedCounter
    };
}
