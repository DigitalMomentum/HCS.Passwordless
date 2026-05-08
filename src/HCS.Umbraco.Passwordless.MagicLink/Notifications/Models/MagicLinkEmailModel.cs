using HCS.Umbraco.Passwordless.Configuration;

namespace HCS.Umbraco.Passwordless.Notifications.Models;

public sealed class MagicLinkEmailModel
{
    public string MemberName { get; init; } = string.Empty;
    public Uri Link { get; init; } = null!;
    public TimeSpan Expiry { get; init; }
    public BrandingOptions Branding { get; init; } = new();
}
