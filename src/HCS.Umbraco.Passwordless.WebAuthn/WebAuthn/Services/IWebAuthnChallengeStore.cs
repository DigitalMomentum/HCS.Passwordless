using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace HCS.Umbraco.Passwordless.WebAuthn.Services;

public interface IWebAuthnChallengeStore
{
    Task PutAsync<T>(string key, T payload, TimeSpan ttl, CancellationToken ct = default);
    Task<T?> TakeAsync<T>(string key, CancellationToken ct = default);
}

internal sealed class DistributedCacheChallengeStore : IWebAuthnChallengeStore
{
    private readonly IDistributedCache _cache;

    public DistributedCacheChallengeStore(IDistributedCache cache) => _cache = cache;

    public async Task PutAsync<T>(string key, T payload, TimeSpan ttl, CancellationToken ct = default)
    {
        var json = JsonSerializer.SerializeToUtf8Bytes(payload);
        await _cache.SetAsync(key, json, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl
        }, ct);
    }

    public async Task<T?> TakeAsync<T>(string key, CancellationToken ct = default)
    {
        var json = await _cache.GetAsync(key, ct);
        if (json is null) return default;
        await _cache.RemoveAsync(key, ct);
        return JsonSerializer.Deserialize<T>(json);
    }
}
