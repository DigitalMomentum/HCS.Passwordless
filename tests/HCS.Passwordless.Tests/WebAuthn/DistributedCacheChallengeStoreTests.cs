using HCS.Passwordless.WebAuthn.Services;

namespace HCS.Passwordless.Tests.WebAuthn;

public class DistributedCacheChallengeStoreTests
{
    private sealed record TestPayload(string Value, int Number);

    private static DistributedCacheChallengeStore CreateStore()
        => new(new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions())));

    [Fact]
    public async Task TakeAsync_ReturnsNull_WhenKeyNeverStored()
    {
        var store = CreateStore();
        var result = await store.TakeAsync<TestPayload>("missing-key");
        result.Should().BeNull();
    }

    [Fact]
    public async Task TakeAsync_ReturnsPayload_AfterPut()
    {
        var store = CreateStore();
        var payload = new TestPayload("hello", 42);

        await store.PutAsync("key1", payload, TimeSpan.FromMinutes(5));
        var result = await store.TakeAsync<TestPayload>("key1");

        result.Should().Be(payload);
    }

    [Fact]
    public async Task TakeAsync_IsSingleUse_SecondTakeReturnsNull()
    {
        var store = CreateStore();
        var payload = new TestPayload("once", 1);

        await store.PutAsync("key2", payload, TimeSpan.FromMinutes(5));
        await store.TakeAsync<TestPayload>("key2");
        var second = await store.TakeAsync<TestPayload>("key2");

        second.Should().BeNull("challenge must be consumed on first take");
    }

    [Fact]
    public async Task PutAsync_OverwritesPreviousPayload()
    {
        var store = CreateStore();
        var first = new TestPayload("first", 1);
        var second = new TestPayload("second", 2);

        await store.PutAsync("key3", first, TimeSpan.FromMinutes(5));
        await store.PutAsync("key3", second, TimeSpan.FromMinutes(5));
        var result = await store.TakeAsync<TestPayload>("key3");

        result.Should().Be(second);
    }

    [Fact]
    public async Task DifferentKeys_AreIsolated()
    {
        var store = CreateStore();
        var payloadA = new TestPayload("A", 1);
        var payloadB = new TestPayload("B", 2);

        await store.PutAsync("keyA", payloadA, TimeSpan.FromMinutes(5));
        await store.PutAsync("keyB", payloadB, TimeSpan.FromMinutes(5));

        (await store.TakeAsync<TestPayload>("keyA")).Should().Be(payloadA);
        (await store.TakeAsync<TestPayload>("keyB")).Should().Be(payloadB);
    }
}
