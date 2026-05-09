using System.Collections.Concurrent;
using HCS.Passwordless.Services;

namespace HCS.Passwordless.Otp.Services;

internal sealed class InMemoryAttemptCounter : IAttemptCounter, IDisposable
{
    private readonly record struct Entry(int Count, DateTimeOffset Expiry);

    private readonly ConcurrentDictionary<string, Entry> _state = new();
    private readonly IPasswordlessClock _clock;
    private readonly Timer _pruneTimer;

    public InMemoryAttemptCounter(IPasswordlessClock clock)
    {
        _clock = clock;
        _pruneTimer = new Timer(_ => Prune(), null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
    }

    public Task<(int Count, bool IsLocked)> IncrementAndCheckAsync(
        string memberId, string purpose, int maxAttempts, TimeSpan lockDuration, CancellationToken ct = default)
    {
        var key = $"{memberId}:{purpose}";
        var now = _clock.UtcNow;
        var expiry = now.Add(lockDuration);

        var result = _state.AddOrUpdate(
            key,
            addValueFactory: _ => new Entry(1, expiry),
            updateValueFactory: (_, existing) =>
            {
                if (existing.Expiry <= now)
                    return new Entry(1, expiry);
                if (existing.Count >= maxAttempts)
                    return existing;
                return new Entry(existing.Count + 1, existing.Expiry);
            });

        return Task.FromResult((result.Count, result.Count >= maxAttempts));
    }

    public Task ResetAsync(string memberId, string purpose, CancellationToken ct = default)
    {
        _state.TryRemove($"{memberId}:{purpose}", out _);
        return Task.CompletedTask;
    }

    private void Prune()
    {
        var now = _clock.UtcNow;
        foreach (var kvp in _state)
            if (kvp.Value.Expiry < now)
                _state.TryRemove(kvp.Key, out _);
    }

    public void Dispose() => _pruneTimer.Dispose();
}
