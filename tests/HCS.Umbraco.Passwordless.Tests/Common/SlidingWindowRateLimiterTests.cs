using HCS.Umbraco.Passwordless.RateLimiting;

namespace HCS.Umbraco.Passwordless.Tests.Common;

public class FixedWindowRateLimiterTests
{
    private static FixedWindowRateLimiter CreateLimiter() => new();

    [Fact]
    public async Task TryAcquireAsync_ReturnsTrueForFirstRequest()
    {
        var limiter = CreateLimiter();
        var result = await limiter.TryAcquireAsync("key1", TimeSpan.FromMinutes(1), limit: 5);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task TryAcquireAsync_AllowsRequestsUpToLimit()
    {
        var limiter = CreateLimiter();
        const int limit = 3;

        for (var i = 0; i < limit; i++)
        {
            var allowed = await limiter.TryAcquireAsync("key2", TimeSpan.FromMinutes(1), limit);
            allowed.Should().BeTrue($"request {i + 1} should be allowed");
        }
    }

    [Fact]
    public async Task TryAcquireAsync_ReturnsFalseAfterLimitReached()
    {
        var limiter = CreateLimiter();
        const int limit = 2;

        await limiter.TryAcquireAsync("key3", TimeSpan.FromMinutes(1), limit);
        await limiter.TryAcquireAsync("key3", TimeSpan.FromMinutes(1), limit);
        var result = await limiter.TryAcquireAsync("key3", TimeSpan.FromMinutes(1), limit);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task TryAcquireAsync_DifferentKeysAreIndependent()
    {
        var limiter = CreateLimiter();
        const int limit = 1;

        await limiter.TryAcquireAsync("key-a", TimeSpan.FromMinutes(1), limit);

        var resultB = await limiter.TryAcquireAsync("key-b", TimeSpan.FromMinutes(1), limit);
        resultB.Should().BeTrue("different key has its own window");
    }

    [Fact]
    public async Task TryAcquireAsync_LimitOfOneDeniesSecondCall()
    {
        var limiter = CreateLimiter();
        await limiter.TryAcquireAsync("single", TimeSpan.FromMinutes(1), limit: 1);
        var second = await limiter.TryAcquireAsync("single", TimeSpan.FromMinutes(1), limit: 1);
        second.Should().BeFalse();
    }
}
