using HCS.Passwordless.Configuration;

namespace HCS.Passwordless.MagicLink.Notifications.Models;

public sealed class MagicLinkEmailModel
{
    public string MemberName { get; init; } = string.Empty;
    public Uri Link { get; init; } = null!;
    public TimeSpan Expiry { get; init; }
    public BrandingOptions Branding { get; init; } = new();
}
