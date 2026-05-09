using Microsoft.Extensions.Caching.Distributed;

namespace HCS.Passwordless.Otp.Services;

internal sealed class DistributedCacheOtpCodeStore : IOtpCodeStore
{
    private readonly IDistributedCache _cache;

    public DistributedCacheOtpCodeStore(IDistributedCache cache) => _cache = cache;

    private static string Key(string memberId, string purpose) => $"pwl:otp:{memberId}:{purpose}";

    public Task SetAsync(string memberId, string purpose, byte[] hash, TimeSpan ttl, CancellationToken ct = default)
        => _cache.SetAsync(Key(memberId, purpose), hash,
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl }, ct);

    public Task<byte[]?> GetAsync(string memberId, string purpose, CancellationToken ct = default)
        => _cache.GetAsync(Key(memberId, purpose), ct);

    public Task DeleteAsync(string memberId, string purpose, CancellationToken ct = default)
        => _cache.RemoveAsync(Key(memberId, purpose), ct);
}
