using HCS.Umbraco.Passwordless.Auth;
using HCS.Umbraco.Passwordless.Configuration;

namespace HCS.Umbraco.Passwordless.Tests.Common;

public class MagicLinkAuthFactorTests
{
    private static IOptionsMonitor<PasswordlessOptions> MonitorFor(bool enabled)
    {
        var opts = new PasswordlessOptions { MagicLink = new MagicLinkOptions { Enabled = enabled } };
        var monitor = Substitute.For<IOptionsMonitor<PasswordlessOptions>>();
        monitor.CurrentValue.Returns(opts);
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
        var opts = new PasswordlessOptions { MagicLink = new MagicLinkOptions { Enabled = true } };
        var monitor = Substitute.For<IOptionsMonitor<PasswordlessOptions>>();
        monitor.CurrentValue.Returns(_ => opts);

        var factor = new MagicLinkAuthFactor(monitor);
        factor.IsEnabled.Should().BeTrue();

        opts.MagicLink.Enabled = false;
        factor.IsEnabled.Should().BeFalse();
    }
}
