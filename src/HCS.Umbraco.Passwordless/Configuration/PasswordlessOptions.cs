namespace HCS.Umbraco.Passwordless.Configuration;

public sealed class PasswordlessOptions
{
    public const string SectionName = "HCS:Authentication";

    public string LoginPath { get; set; } = "/login";
    public string PostLoginRedirectPath { get; set; } = "/";
    public bool RejectUnknownEmails { get; set; } = true;

    public MagicLinkOptions MagicLink { get; set; } = new();
    public RateLimitOptions RateLimits { get; set; } = new();
    public NotificationOptions Notifications { get; set; } = new();
}
