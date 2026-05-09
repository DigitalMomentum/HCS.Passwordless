using HCS.Umbraco.Passwordless.Services;
using NSubstitute;

namespace HCS.Umbraco.Passwordless.Tests.Common;

public class InMemorySingleUseTokenStoreTests
{
    private static InMemorySingleUseTokenStore CreateStore(DateTimeOffset? now = null)
    {
        var clock = Substitute.For<IPasswordlessClock>();
        clock.UtcNow.Returns(now ?? DateTimeOffset.UtcNow);
        return new InMemorySingleUseTokenStore(clock);
    }

    [Fact]
    public async Task TryMarkUsedAsync_ReturnsTrueForNewToken()
    {
        var store = CreateStore();
        var result = await store.TryMarkUsedAsync("token-hash-1", TimeSpan.FromMinutes(15));
        result.Should().BeTrue();
    }

    [Fact]
    public async Task TryMarkUsedAsync_ReturnsFalseForAlreadyUsedToken()
    {
        var store = CreateStore();
        const string hash = "token-hash-2";

        await store.TryMarkUsedAsync(hash, TimeSpan.FromMinutes(15));
        var second = await store.TryMarkUsedAsync(hash, TimeSpan.FromMinutes(15));

        second.Should().BeFalse();
    }

    [Fact]
    public async Task TryMarkUsedAsync_DifferentHashesAreIndependent()
    {
        var store = CreateStore();

        var first = await store.TryMarkUsedAsync("hash-a", TimeSpan.FromMinutes(15));
        var second = await store.TryMarkUsedAsync("hash-b", TimeSpan.FromMinutes(15));

        first.Should().BeTrue();
        second.Should().BeTrue();
    }

    [Fact]
    public async Task TryMarkUsedAsync_ReturnsTrue_ThenFalse_ForSameHash()
    {
        var store = CreateStore();
        const string hash = "replay-hash";

        var first = await store.TryMarkUsedAsync(hash, TimeSpan.FromMinutes(1));
        var replay = await store.TryMarkUsedAsync(hash, TimeSpan.FromMinutes(1));

        first.Should().BeTrue();
        replay.Should().BeFalse("token replay must be rejected");
    }

    [Fact]
    public async Task TryMarkUsedAsync_ConcurrentCalls_OnlyOneSucceeds()
    {
        var store = CreateStore();
        const string hash = "concurrent-hash";

        var results = await Task.WhenAll(
            Enumerable.Range(0, 50).Select(_ => store.TryMarkUsedAsync(hash, TimeSpan.FromMinutes(15))));

        results.Count(r => r).Should().Be(1, "exactly one concurrent caller should win the TryAdd race");
    }

    [Fact]
    public async Task TryMarkUsedAsync_ConcurrentCallsOnDifferentHashes_AllSucceed()
    {
        var store = CreateStore();

        var results = await Task.WhenAll(
            Enumerable.Range(0, 50).Select(i => store.TryMarkUsedAsync($"hash-{i}", TimeSpan.FromMinutes(15))));

        results.Should().AllSatisfy(r => r.Should().BeTrue("each unique hash should succeed"));
    }
}
