namespace HCS.Passwordless.WebAuthn.Storage;

public sealed record StoredCredential(
    Guid Id,
    Guid MemberKey,
    byte[] CredentialId,
    byte[] PublicKey,
    byte[] UserHandle,
    uint SignatureCounter,
    string CredType,
    Guid AaGuid,
    string? Transports,
    bool BackupEligible,
    bool BackupState,
    string? Nickname,
    DateTime CreatedUtc,
    DateTime? LastUsedUtc,
    string? AttestationFormat,
    bool HasEverIncrementedCounter = false);
