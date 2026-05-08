namespace HCS.Umbraco.Passwordless.Configuration;

public sealed class RateLimitOptions
{
    public int PerIpRequestsPerMinute { get; set; } = 10;
    public int PerEmailRequestsPerHour { get; set; } = 5;
    public int VerifyPerIpPerMinute { get; set; } = 20;
    public TimeSpan FakeWorkDelay { get; set; } = TimeSpan.FromMilliseconds(250);
}
