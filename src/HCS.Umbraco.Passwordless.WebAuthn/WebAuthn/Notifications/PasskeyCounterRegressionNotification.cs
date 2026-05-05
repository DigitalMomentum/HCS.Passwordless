using Umbraco.Cms.Core.Notifications;

namespace HCS.Umbraco.Passwordless.WebAuthn.Notifications;

public sealed class PasskeyCounterRegressionNotification : INotification
{
    public Guid MemberKey { get; init; }
    public byte[] CredentialId { get; init; } = Array.Empty<byte>();
    public uint StoredCounter { get; init; }
    public uint ReceivedCounter { get; init; }
}
