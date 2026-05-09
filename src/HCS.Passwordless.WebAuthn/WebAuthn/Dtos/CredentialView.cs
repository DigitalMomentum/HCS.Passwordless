namespace HCS.Passwordless.WebAuthn.Dtos;

public sealed record CredentialView(
    Guid Id,
    string? Nickname,
    string? FriendlyName,
    Guid AaGuid,
    DateTime CreatedUtc,
    DateTime? LastUsedUtc,
    bool BackupEligible,
    bool BackupState,
    string? Transports);
