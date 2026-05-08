using Microsoft.Extensions.Caching.Distributed;

namespace HCS.Umbraco.Passwordless.Services;

internal sealed class DistributedCacheSingleUseTokenStore : ISingleUseTokenStore
{
    private readonly IDistributedCache _cache;

    public DistributedCacheSingleUseTokenStore(IDistributedCache cache) => _cache = cache;

    public async Task<bool> TryMarkUsedAsync(string tokenHash, TimeSpan ttl, CancellationToken ct = default)
    {
        var key = $"pwl:ml-used:{tokenHash}";
        var existing = await _cache.GetStringAsync(key, ct);
        if (existing is not null) return false;

        await _cache.SetStringAsync(key, "1", new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl + TimeSpan.FromSeconds(60)
        }, ct);
        return true;
    }
}
