using System.Security.Cryptography;

namespace HCS.Umbraco.Passwordless.Endpoints.Shared;

internal static class FakeWork
{
    public static Task DelayAsync(TimeSpan budget, CancellationToken ct = default)
    {
        // Jitter: 50%–100% of the budget to approximate real path timing.
        var half = (int)(budget.TotalMilliseconds / 2);
        var ms = half + RandomNumberGenerator.GetInt32(half + 1);
        return Task.Delay(ms, ct);
    }
}
