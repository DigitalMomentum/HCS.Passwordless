using HCS.Passwordless.Configuration;

namespace HCS.Passwordless.Otp.Notifications.Models;

public sealed class OtpEmailModel
{
    public string MemberName { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public TimeSpan Expiry { get; init; }
    public BrandingOptions Branding { get; init; } = new();
}
