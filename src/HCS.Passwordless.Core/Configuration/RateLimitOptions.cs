namespace HCS.Passwordless.Configuration;

/// <summary>Rate-limiting thresholds applied to authentication endpoints.</summary>
public sealed class RateLimitOptions
{
    /// <summary>Maximum authentication requests allowed per IP address per minute.</summary>
    public int PerIpRequestsPerMinute { get; set; } = 10;

    /// <summary>Maximum authentication requests allowed per email address per hour.</summary>
    public int PerEmailRequestsPerHour { get; set; } = 5;

    /// <summary>Maximum token-verify attempts allowed per IP address per minute.</summary>
    public int VerifyPerIpPerMinute { get; set; } = 20;

    /// <summary>Minimum response time enforced on all authentication endpoints to mitigate timing attacks.</summary>
    public TimeSpan FakeWorkDelay { get; set; } = TimeSpan.FromMilliseconds(250);
}
