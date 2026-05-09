using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace HCS.Umbraco.Passwordless.WebAuthn.Services;

/// <summary>
/// Stores and atomically consumes single-use WebAuthn ceremony state
/// (registration and sign-in challenges).
/// </summary>
/// <remarks>
/// <para>
/// The default implementation (<c>DistributedCacheChallengeStore</c>) stores
/// challenges via <c>IDistributedCache</c> and removes the entry on retrieval.
/// However, <c>IDistributedCache</c> does not expose atomic get-and-delete, so
/// the default implementation has a narrow TOCTOU window: two simultaneous
/// <c>TakeAsync</c> calls with the same key can both retrieve the challenge
/// before either deletes it.
/// </para>
/// <para>
/// In practice this window is very narrow and the FIDO2 library provides a
/// secondary defence (signature replay is rejected by counter/signature checks).
/// For high-security deployments the window can be closed entirely by replacing
/// this service with an atomic implementation using Redis <c>GETDEL</c>.
/// </para>
/// <para>
/// When a real shared <c>IDistributedCache</c> (e.g. Redis via
/// <c>AddStackExchangeRedisCache</c>) is configured, the default implementation
/// coordinates correctly across nodes — the TOCTOU window is the only remaining
/// concern and only matters under intentional concurrent-replay attack.
/// </para>
/// <para>
/// Register a replacement via <c>WebAuthnBuilder.UseChallengeStore&lt;T&gt;()</c>.
/// See the <em>Multi-Instance Deployments</em> guide for a Redis reference
/// implementation using <c>GETDEL</c>.
/// </para>
/// </remarks>
public interface IWebAuthnChallengeStore
{
    /// <summary>Stores <paramref name="payload"/> under <paramref name="key"/> with the given TTL.</summary>
    Task PutAsync<T>(string key, T payload, TimeSpan ttl, CancellationToken ct = default);

    /// <summary>
    /// Atomically retrieves and removes the entry for <paramref name="key"/>.
    /// Returns <c>null</c> if the key does not exist or has expired.
    /// Implementations must ensure only one caller can successfully retrieve each key.
    /// </summary>
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
