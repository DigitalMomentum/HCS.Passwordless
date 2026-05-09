using HCS.Passwordless.Otp.Services;
using HCS.Passwordless.Services;
using NSubstitute;

namespace HCS.Passwordless.Tests.Otp;

public class InMemoryAttemptCounterTests
{
    private static InMemoryAttemptCounter CreateCounter(DateTimeOffset? now = null)
    {
        var clock = Substitute.For<IPasswordlessClock>();
        clock.UtcNow.Returns(now ?? DateTimeOffset.UtcNow);
        return new InMemoryAttemptCounter(clock);
    }

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

        await counter.IncrementAndCheckAsync("m4", "p", maxAttempts, TimeSpan.FromMinutes(15));
        await counter.IncrementAndCheckAsync("m4", "p", maxAttempts, TimeSpan.FromMinutes(15));

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

        for (var i = 0; i < maxAttempts; i++)
            await counter.IncrementAndCheckAsync("alice", "p", maxAttempts, TimeSpan.FromMinutes(15));

        var (count, isLocked) = await counter.IncrementAndCheckAsync("bob", "p", maxAttempts, TimeSpan.FromMinutes(15));
        count.Should().Be(1);
        isLocked.Should().BeFalse("different member ID should have its own counter");
    }

    [Fact]
    public async Task IncrementAndCheckAsync_ConcurrentCalls_EachGetsUniqueMonotonicCount()
    {
        var counter = CreateCounter();
        const int threads = 50;
        const int maxAttempts = 100;

        var results = await Task.WhenAll(
            Enumerable.Range(0, threads)
                .Select(_ => counter.IncrementAndCheckAsync("m-concurrent", "p", maxAttempts, TimeSpan.FromMinutes(15))));

        var counts = results.Select(r => r.Count).OrderBy(c => c).ToList();
        counts.Should().Equal(Enumerable.Range(1, threads),
            "every concurrent increment must land on a unique count — no two threads share a value");
    }

    [Fact]
    public async Task IncrementAndCheckAsync_ConcurrentCalls_LockoutEnforcedExactlyAtMax()
    {
        var counter = CreateCounter();
        const int maxAttempts = 10;
        const int threads = 50;

        var results = await Task.WhenAll(
            Enumerable.Range(0, threads)
                .Select(_ => counter.IncrementAndCheckAsync("m-lockout", "p", maxAttempts, TimeSpan.FromMinutes(15))));

        var lockedCount = results.Count(r => r.IsLocked);
        lockedCount.Should().Be(threads - maxAttempts + 1,
            "exactly the calls at and beyond maxAttempts should be locked");
    }
}
