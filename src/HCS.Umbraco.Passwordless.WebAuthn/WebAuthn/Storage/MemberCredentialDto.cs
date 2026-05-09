using NPoco;

namespace HCS.Umbraco.Passwordless.WebAuthn.Storage;

[TableName("Passwordless_MemberCredentials")]
[PrimaryKey("Id", AutoIncrement = false)]
internal sealed class MemberCredentialDto
{
    public Guid Id { get; set; }
    public Guid MemberKey { get; set; }
    public byte[] CredentialId { get; set; } = Array.Empty<byte>();
    public byte[] PublicKey { get; set; } = Array.Empty<byte>();
    public byte[] UserHandle { get; set; } = Array.Empty<byte>();
    public long SignatureCounter { get; set; }
    public string CredType { get; set; } = string.Empty;
    public Guid AaGuid { get; set; }
    public string? Transports { get; set; }
    public bool BackupEligible { get; set; }
    public bool BackupState { get; set; }
    public string? Nickname { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime? LastUsedUtc { get; set; }
    public string? AttestationFormat { get; set; }
    public bool HasEverIncrementedCounter { get; set; }
}
