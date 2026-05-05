using HCS.Umbraco.Passwordless.Otp.Services;

namespace HCS.Umbraco.Passwordless.Tests.Otp;

public class DistributedCacheOtpCodeStoreTests
{
    private static DistributedCacheOtpCodeStore CreateStore()
        => new(new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions())));

    [Fact]
    public async Task GetAsync_ReturnsNull_WhenNothingStored()
    {
        var store = CreateStore();
        var result = await store.GetAsync("m1", "purpose");
        result.Should().BeNull();
    }

    [Fact]
    public async Task SetAsync_Then_GetAsync_ReturnsSameHash()
    {
        var store = CreateStore();
        var hash = new byte[] { 1, 2, 3, 4, 5 };

        await store.SetAsync("m1", "purpose", hash, TimeSpan.FromMinutes(5));
        var result = await store.GetAsync("m1", "purpose");

        result.Should().Equal(hash);
    }

    [Fact]
    public async Task DeleteAsync_RemovesStoredHash()
    {
        var store = CreateStore();
        var hash = new byte[] { 9, 8, 7 };

        await store.SetAsync("m2", "purpose", hash, TimeSpan.FromMinutes(5));
        await store.DeleteAsync("m2", "purpose");
        var result = await store.GetAsync("m2", "purpose");

        result.Should().BeNull();
    }

    [Fact]
    public async Task DifferentMemberAndPurposeCombinationsAreIsolated()
    {
        var store = CreateStore();
        var hashA = new byte[] { 1 };
        var hashB = new byte[] { 2 };

        await store.SetAsync("alice", "login", hashA, TimeSpan.FromMinutes(5));
        await store.SetAsync("bob", "login", hashB, TimeSpan.FromMinutes(5));

        (await store.GetAsync("alice", "login")).Should().Equal(hashA);
        (await store.GetAsync("bob", "login")).Should().Equal(hashB);
    }

    [Fact]
    public async Task SetAsync_OverwritesPreviousHash()
    {
        var store = CreateStore();
        var first = new byte[] { 1, 2, 3 };
        var second = new byte[] { 4, 5, 6 };

        await store.SetAsync("m3", "p", first, TimeSpan.FromMinutes(5));
        await store.SetAsync("m3", "p", second, TimeSpan.FromMinutes(5));

        (await store.GetAsync("m3", "p")).Should().Equal(second);
    }
}
