namespace HCS.Passwordless.Configuration;

/// <summary>Root configuration for the HCS Passwordless suite. Binds from <c>HCS:Authentication</c>.</summary>
public sealed class PasswordlessOptions
{
    /// <summary>The configuration section name: <c>HCS:Authentication</c>.</summary>
    public const string SectionName = "HCS:Authentication";

    /// <summary>URL of the login page members are redirected to when unauthenticated.</summary>
    public string LoginPath { get; set; } = "/login";

    /// <summary>URL to redirect to after a successful sign-in.</summary>
    public string PostLoginRedirectPath { get; set; } = "/";

    /// <summary>Rate-limiting thresholds for authentication endpoints.</summary>
    public RateLimitOptions RateLimits { get; set; } = new();

    /// <summary>Email notification settings shared across all authentication factors.</summary>
    public NotificationOptions Notifications { get; set; } = new();
}
