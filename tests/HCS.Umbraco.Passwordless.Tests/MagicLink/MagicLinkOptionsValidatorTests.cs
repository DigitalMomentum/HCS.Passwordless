using HCS.Umbraco.Passwordless.Configuration;

namespace HCS.Umbraco.Passwordless.Tests.MagicLink;

public class MagicLinkOptionsValidatorTests
{
    private readonly MagicLinkOptionsValidator _sut = new();

    [Fact]
    public void Validate_Succeeds_ForValidDefaults()
    {
        var result = _sut.Validate(null, new MagicLinkOptions());
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validate_Fails_WhenEnabledAndTokenLifespanIsZero()
    {
        var opts = new MagicLinkOptions { Enabled = true, TokenLifespan = TimeSpan.Zero };
        var result = _sut.Validate(null, opts);
        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should().Contain("TokenLifespan");
    }

    [Fact]
    public void Validate_Fails_WhenEnabledAndTokenLifespanIsNegative()
    {
        var opts = new MagicLinkOptions { Enabled = true, TokenLifespan = TimeSpan.FromSeconds(-1) };
        var result = _sut.Validate(null, opts);
        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public void Validate_Succeeds_WhenDisabledAndTokenLifespanIsZero()
    {
        var opts = new MagicLinkOptions { Enabled = false, TokenLifespan = TimeSpan.Zero };
        var result = _sut.Validate(null, opts);
        result.Succeeded.Should().BeTrue("disabled magic link should skip lifespan validation");
    }
}
