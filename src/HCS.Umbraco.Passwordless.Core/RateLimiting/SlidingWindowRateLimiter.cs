using Microsoft.Extensions.Caching.Memory;

namespace HCS.Umbraco.Passwordless.RateLimiting;

// Named FixedWindowRateLimiter because it divides time into fixed epoch-second buckets.
// Allows up to 2× burst at window boundaries — documented accepted behaviour (L-1).
internal sealed class FixedWindowRateLimiter : IPasswordlessRateLimiter
{
    private readonly IMemoryCache _cache;

    public FixedWindowRateLimiter(IMemoryCache cache) => _cache = cache;

    public Task<bool> TryAcquireAsync(string key, TimeSpan window, int limit, CancellationToken ct = default)
    {
        var windowStart = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / (long)window.TotalSeconds;
        var cacheKey = $"pwl:rate:{key}:{windowStart}";

        var count = _cache.GetOrCreate(cacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = window * 2;
            return 0;
        });

        if (count >= limit) return Task.FromResult(false);

        _cache.Set(cacheKey, count + 1, window * 2);
        return Task.FromResult(true);
    }
}
