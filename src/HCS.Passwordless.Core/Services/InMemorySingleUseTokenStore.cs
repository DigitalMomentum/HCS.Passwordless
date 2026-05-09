using System.Collections.Concurrent;

namespace HCS.Passwordless.Services;

internal sealed class InMemorySingleUseTokenStore : ISingleUseTokenStore, IDisposable
{
    private readonly ConcurrentDictionary<string, DateTimeOffset> _used = new();
    private readonly IPasswordlessClock _clock;
    private readonly Timer _pruneTimer;

    public InMemorySingleUseTokenStore(IPasswordlessClock clock)
    {
        _clock = clock;
        _pruneTimer = new Timer(_ => Prune(), null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
    }

    public Task<bool> TryMarkUsedAsync(string tokenHash, TimeSpan ttl, CancellationToken ct = default)
    {
        var expiry = _clock.UtcNow.Add(ttl).Add(TimeSpan.FromSeconds(60));
        return Task.FromResult(_used.TryAdd(tokenHash, expiry));
    }

    private void Prune()
    {
        var now = _clock.UtcNow;
        foreach (var kvp in _used)
            if (kvp.Value < now)
                _used.TryRemove(kvp.Key, out _);
    }

    public void Dispose() => _pruneTimer.Dispose();
}
