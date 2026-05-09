using Microsoft.Extensions.Caching.Distributed;

namespace HCS.Passwordless.Otp.Services;

internal sealed class DistributedCacheAttemptCounter : IAttemptCounter
{
    private readonly IDistributedCache _cache;

    public DistributedCacheAttemptCounter(IDistributedCache cache) => _cache = cache;

    public async Task<(int Count, bool IsLocked)> IncrementAndCheckAsync(
        string memberId, string purpose, int maxAttempts, TimeSpan lockDuration, CancellationToken ct = default)
    {
        var lockKey = $"pwl:attempts-lock:{memberId}:{purpose}";
        if (await _cache.GetStringAsync(lockKey, ct) is not null) return (maxAttempts, true);

        var countKey = $"pwl:attempts:{memberId}:{purpose}";
        var raw = await _cache.GetStringAsync(countKey, ct);
        var count = raw is null ? 0 : int.Parse(raw);
        count++;

        var opts = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = lockDuration };
        await _cache.SetStringAsync(countKey, count.ToString(), opts, ct);

        if (count >= maxAttempts)
        {
            await _cache.SetStringAsync(lockKey, "1", opts, ct);
            return (count, true);
        }
        return (count, false);
    }

    public async Task ResetAsync(string memberId, string purpose, CancellationToken ct = default)
    {
        await _cache.RemoveAsync($"pwl:attempts:{memberId}:{purpose}", ct);
        await _cache.RemoveAsync($"pwl:attempts-lock:{memberId}:{purpose}", ct);
    }
}
