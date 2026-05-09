using HCS.Umbraco.Passwordless.WebAuthn.Configuration;
using Microsoft.Extensions.Hosting;

namespace HCS.Umbraco.Passwordless.Tests.WebAuthn;

public class WebAuthnOptionsValidatorTests
{
    private readonly IHostEnvironment _env = Substitute.For<IHostEnvironment>();

    private WebAuthnOptionsValidator CreateSut() => new(_env);

    private static WebAuthnOptions EnabledWithKey(string key = "0123456789abcdef") =>
        new() { Enabled = true, DecoyHmacKey = key };

    [Fact]
    public void Validate_Succeeds_WhenDisabled()
    {
        _env.EnvironmentName.Returns("Production");
        var result = CreateSut().Validate(null, new WebAuthnOptions { Enabled = false });
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validate_Succeeds_WhenEnabledInProductionWithOriginsAndKey()
    {
        _env.EnvironmentName.Returns("Production");
        var opts = EnabledWithKey();
        opts.Origins.Add("https://example.com");
        var result = CreateSut().Validate(null, opts);
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validate_Fails_WhenEnabledInProductionWithNoOrigins()
    {
        _env.EnvironmentName.Returns("Production");
        var opts = EnabledWithKey();
        var result = CreateSut().Validate(null, opts);
        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should().Contain("Origins");
    }

    [Fact]
    public void Validate_Succeeds_WhenEnabledInDevelopmentWithNoOrigins()
    {
        _env.EnvironmentName.Returns("Development");
        var opts = EnabledWithKey();
        var result = CreateSut().Validate(null, opts);
        result.Succeeded.Should().BeTrue("Origins are not required outside Production");
    }

    [Theory]
    [InlineData("Development")]
    [InlineData("Staging")]
    public void Validate_Succeeds_ForNonProductionEnvironments_WhenOriginsEmpty(string envName)
    {
        _env.EnvironmentName.Returns(envName);
        var opts = EnabledWithKey();
        var result = CreateSut().Validate(null, opts);
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validate_Fails_WhenDecoyHmacKeyIsNull()
    {
        _env.EnvironmentName.Returns("Development");
        var opts = new WebAuthnOptions { Enabled = true, DecoyHmacKey = null };
        var result = CreateSut().Validate(null, opts);
        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should().Contain("DecoyHmacKey");
    }

    [Fact]
    public void Validate_Fails_WhenDecoyHmacKeyIsEmpty()
    {
        _env.EnvironmentName.Returns("Development");
        var opts = new WebAuthnOptions { Enabled = true, DecoyHmacKey = "" };
        var result = CreateSut().Validate(null, opts);
        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should().Contain("DecoyHmacKey");
    }

    [Fact]
    public void Validate_Fails_WhenDecoyHmacKeyIsTooShort()
    {
        _env.EnvironmentName.Returns("Development");
        var opts = new WebAuthnOptions { Enabled = true, DecoyHmacKey = "tooshort" }; // 8 bytes
        var result = CreateSut().Validate(null, opts);
        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should().Contain("DecoyHmacKey");
    }

    [Fact]
    public void Validate_Succeeds_WhenDecoyHmacKeyIsExactlyMinLength()
    {
        _env.EnvironmentName.Returns("Development");
        var opts = new WebAuthnOptions { Enabled = true, DecoyHmacKey = "exactly16bytes!!" }; // 16 bytes
        var result = CreateSut().Validate(null, opts);
        result.Succeeded.Should().BeTrue();
    }
}
