using System.Collections.Concurrent;

namespace HCS.Umbraco.Passwordless.RateLimiting;

// Named FixedWindowRateLimiter because it divides time into fixed epoch-second buckets.
// Allows up to 2× burst at window boundaries — documented accepted behaviour (L-1).
//
// Atomicity: each bucket is a small class whose Count field is incremented with
// Interlocked.Increment. ConcurrentDictionary.GetOrAdd guarantees every caller
// receives the same Bucket reference for a given key, so the increment is
// race-free even under concurrent load — unlike the previous IMemoryCache
// GetOrCreate+Set pattern, which had a TOCTOU gap (N-L4).
internal sealed class FixedWindowRateLimiter : IPasswordlessRateLimiter, IDisposable
{
    private sealed class Bucket(long expiresAtTicks)
    {
        public readonly long ExpiresAtTicks = expiresAtTicks;
        public int Count;
    }

    private readonly ConcurrentDictionary<string, Bucket> _buckets = new();
    private readonly Timer _pruneTimer;

    public FixedWindowRateLimiter()
    {
        _pruneTimer = new Timer(_ => Prune(), null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
    }

    public Task<bool> TryAcquireAsync(string key, TimeSpan window, int limit, CancellationToken ct = default)
    {
        var windowStart = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / (long)window.TotalSeconds;
        var cacheKey = $"pwl:rate:{key}:{windowStart}";
        var expiresAt = DateTime.UtcNow.Add(window * 2).Ticks;

        var bucket = _buckets.GetOrAdd(cacheKey, _ => new Bucket(expiresAt));
        var newCount = Interlocked.Increment(ref bucket.Count);
        return Task.FromResult(newCount <= limit);
    }

    private void Prune()
    {
        var now = DateTime.UtcNow.Ticks;
        foreach (var (key, bucket) in _buckets)
            if (bucket.ExpiresAtTicks < now)
                _buckets.TryRemove(key, out _);
    }

    public void Dispose() => _pruneTimer.Dispose();
}
