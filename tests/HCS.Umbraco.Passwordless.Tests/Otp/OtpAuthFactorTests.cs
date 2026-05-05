using HCS.Umbraco.Passwordless.Otp.Auth;
using HCS.Umbraco.Passwordless.Otp.Configuration;

namespace HCS.Umbraco.Passwordless.Tests.Otp;

public class OtpAuthFactorTests
{
    private static IOptionsMonitor<OtpOptions> MonitorFor(bool enabled)
    {
        var opts = new OtpOptions { Enabled = enabled };
        var monitor = Substitute.For<IOptionsMonitor<OtpOptions>>();
        monitor.CurrentValue.Returns(opts);
        return monitor;
    }

    [Fact]
    public void Name_IsConstant()
        => new OtpAuthFactor(MonitorFor(true)).Name.Should().Be("otp");

    [Fact]
    public void IsEnabled_ReturnsTrueWhenOtpEnabled()
        => new OtpAuthFactor(MonitorFor(true)).IsEnabled.Should().BeTrue();

    [Fact]
    public void IsEnabled_ReturnsFalseWhenOtpDisabled()
        => new OtpAuthFactor(MonitorFor(false)).IsEnabled.Should().BeFalse();

    [Fact]
    public void IsEnabled_ReflectsLiveOptionChange()
    {
        var opts = new OtpOptions { Enabled = true };
        var monitor = Substitute.For<IOptionsMonitor<OtpOptions>>();
        monitor.CurrentValue.Returns(_ => opts);

        var factor = new OtpAuthFactor(monitor);
        factor.IsEnabled.Should().BeTrue();

        opts.Enabled = false;
        factor.IsEnabled.Should().BeFalse();
    }
}
