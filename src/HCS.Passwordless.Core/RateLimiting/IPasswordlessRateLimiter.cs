namespace HCS.Passwordless.RateLimiting;

/// <summary>Sliding-window rate limiter used by authentication endpoints.</summary>
public interface IPasswordlessRateLimiter
{
    /// <summary>
    /// Attempts to acquire a rate-limit token for <paramref name="key"/> within the given <paramref name="window"/>.
    /// Returns <c>true</c> if the request is within the allowed <paramref name="limit"/>; <c>false</c> if it should be rejected.
    /// </summary>
    Task<bool> TryAcquireAsync(string key, TimeSpan window, int limit, CancellationToken ct = default);
}
