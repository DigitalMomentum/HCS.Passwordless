using HCS.Umbraco.Passwordless.Services;

namespace HCS.Umbraco.Passwordless.Tests.Common;

public class DistributedCacheSingleUseTokenStoreTests
{
    private static DistributedCacheSingleUseTokenStore CreateStore()
        => new(new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions())));

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
}
