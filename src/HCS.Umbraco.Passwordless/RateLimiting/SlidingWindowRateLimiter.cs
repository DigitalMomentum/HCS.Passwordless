using Microsoft.Extensions.Caching.Memory;

namespace HCS.Umbraco.Passwordless.RateLimiting;

internal sealed class SlidingWindowRateLimiter : IPasswordlessRateLimiter
{
    private readonly IMemoryCache _cache;

    public SlidingWindowRateLimiter(IMemoryCache cache) => _cache = cache;

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
