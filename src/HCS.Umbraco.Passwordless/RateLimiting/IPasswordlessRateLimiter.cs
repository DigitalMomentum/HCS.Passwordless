namespace HCS.Umbraco.Passwordless.RateLimiting;

public interface IPasswordlessRateLimiter
{
    Task<bool> TryAcquireAsync(string key, TimeSpan window, int limit, CancellationToken ct = default);
}
