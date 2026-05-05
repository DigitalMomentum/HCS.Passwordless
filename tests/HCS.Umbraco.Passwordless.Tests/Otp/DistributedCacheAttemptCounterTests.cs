using HCS.Umbraco.Passwordless.Otp.Services;

namespace HCS.Umbraco.Passwordless.Tests.Otp;

public class DistributedCacheAttemptCounterTests
{
    private static DistributedCacheAttemptCounter CreateCounter()
        => new(new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions())));

    [Fact]
    public async Task IncrementAndCheckAsync_ReturnsCount1AndNotLocked_OnFirstCall()
    {
        var counter = CreateCounter();
        var (count, isLocked) = await counter.IncrementAndCheckAsync("m1", "purpose", maxAttempts: 5, TimeSpan.FromMinutes(15));
        count.Should().Be(1);
        isLocked.Should().BeFalse();
    }

    [Fact]
    public async Task IncrementAndCheckAsync_IncrementsCountOnEachCall()
    {
        var counter = CreateCounter();
        const int maxAttempts = 5;

        for (var i = 1; i <= 3; i++)
        {
            var (count, _) = await counter.IncrementAndCheckAsync("m2", "purpose", maxAttempts, TimeSpan.FromMinutes(15));
            count.Should().Be(i);
        }
    }

    [Fact]
    public async Task IncrementAndCheckAsync_LocksMemberWhenMaxAttemptsReached()
    {
        var counter = CreateCounter();
        const int maxAttempts = 3;

        (int _, bool isLocked) last = default;
        for (var i = 0; i < maxAttempts; i++)
            last = await counter.IncrementAndCheckAsync("m3", "purpose", maxAttempts, TimeSpan.FromMinutes(15));

        last.isLocked.Should().BeTrue("member should be locked after max attempts");
    }

    [Fact]
    public async Task IncrementAndCheckAsync_ReturnsLockedImmediately_WhenAlreadyLocked()
    {
        var counter = CreateCounter();
        const int maxAttempts = 2;

        // exhaust attempts to trigger lock
        await counter.IncrementAndCheckAsync("m4", "p", maxAttempts, TimeSpan.FromMinutes(15));
        await counter.IncrementAndCheckAsync("m4", "p", maxAttempts, TimeSpan.FromMinutes(15));

        // any subsequent call should see the lock
        var (count, isLocked) = await counter.IncrementAndCheckAsync("m4", "p", maxAttempts, TimeSpan.FromMinutes(15));
        isLocked.Should().BeTrue();
        count.Should().Be(maxAttempts, "count should not increase past max when locked");
    }

    [Fact]
    public async Task ResetAsync_ClearsCountAndLock()
    {
        var counter = CreateCounter();
        const int maxAttempts = 2;

        await counter.IncrementAndCheckAsync("m5", "p", maxAttempts, TimeSpan.FromMinutes(15));
        await counter.IncrementAndCheckAsync("m5", "p", maxAttempts, TimeSpan.FromMinutes(15));

        await counter.ResetAsync("m5", "p");

        var (count, isLocked) = await counter.IncrementAndCheckAsync("m5", "p", maxAttempts, TimeSpan.FromMinutes(15));
        count.Should().Be(1);
        isLocked.Should().BeFalse("counter should be reset");
    }

    [Fact]
    public async Task IncrementAndCheckAsync_DifferentMembersAreIsolated()
    {
        var counter = CreateCounter();
        const int maxAttempts = 5;

        // exhaust alice's limit
        for (var i = 0; i < maxAttempts; i++)
            await counter.IncrementAndCheckAsync("alice", "p", maxAttempts, TimeSpan.FromMinutes(15));

        // bob's first attempt should not be affected by alice's lock
        var (count, isLocked) = await counter.IncrementAndCheckAsync("bob", "p", maxAttempts, TimeSpan.FromMinutes(15));
        count.Should().Be(1);
        isLocked.Should().BeFalse("different member ID should have its own counter");
    }
}
