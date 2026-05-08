using HCS.Umbraco.Passwordless.WebAuthn.Auth;
using HCS.Umbraco.Passwordless.WebAuthn.Configuration;

namespace HCS.Umbraco.Passwordless.Tests.WebAuthn;

public class WebAuthnAuthFactorTests
{
    private readonly IOptionsMonitor<WebAuthnOptions> _monitor = Substitute.For<IOptionsMonitor<WebAuthnOptions>>();

    private WebAuthnAuthFactor CreateSut() => new(_monitor);

    [Fact]
    public void Name_IsWebauthn()
    {
        _monitor.CurrentValue.Returns(new WebAuthnOptions());
        CreateSut().Name.Should().Be("webauthn");
    }

    [Fact]
    public void IsEnabled_ReturnsTrue_WhenOptionsEnabled()
    {
        _monitor.CurrentValue.Returns(new WebAuthnOptions { Enabled = true });
        CreateSut().IsEnabled.Should().BeTrue();
    }

    [Fact]
    public void IsEnabled_ReturnsFalse_WhenOptionsDisabled()
    {
        _monitor.CurrentValue.Returns(new WebAuthnOptions { Enabled = false });
        CreateSut().IsEnabled.Should().BeFalse();
    }

    [Fact]
    public void IsEnabled_ReflectsCurrentValue_AfterOptionsChange()
    {
        var sut = CreateSut();

        _monitor.CurrentValue.Returns(new WebAuthnOptions { Enabled = true });
        sut.IsEnabled.Should().BeTrue();

        _monitor.CurrentValue.Returns(new WebAuthnOptions { Enabled = false });
        sut.IsEnabled.Should().BeFalse();
    }
}
