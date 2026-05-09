using HCS.Passwordless.MagicLink.Auth;
using HCS.Passwordless.MagicLink.Configuration;

namespace HCS.Passwordless.Tests.Common;

public class MagicLinkAuthFactorTests
{
    private static IOptionsMonitor<MagicLinkOptions> MonitorFor(bool enabled)
    {
        var monitor = Substitute.For<IOptionsMonitor<MagicLinkOptions>>();
        monitor.CurrentValue.Returns(new MagicLinkOptions { Enabled = enabled });
        return monitor;
    }

    [Fact]
    public void Name_IsConstant()
        => new MagicLinkAuthFactor(MonitorFor(true)).Name.Should().Be("magic-link");

    [Fact]
    public void IsEnabled_ReturnsTrueWhenMagicLinkEnabled()
        => new MagicLinkAuthFactor(MonitorFor(true)).IsEnabled.Should().BeTrue();

    [Fact]
    public void IsEnabled_ReturnsFalseWhenMagicLinkDisabled()
        => new MagicLinkAuthFactor(MonitorFor(false)).IsEnabled.Should().BeFalse();

    [Fact]
    public void IsEnabled_ReflectsLiveOptionChange()
    {
        var mlOpts = new MagicLinkOptions { Enabled = true };
        var monitor = Substitute.For<IOptionsMonitor<MagicLinkOptions>>();
        monitor.CurrentValue.Returns(_ => mlOpts);

        var factor = new MagicLinkAuthFactor(monitor);
        factor.IsEnabled.Should().BeTrue();

        mlOpts.Enabled = false;
        factor.IsEnabled.Should().BeFalse();
    }
}
