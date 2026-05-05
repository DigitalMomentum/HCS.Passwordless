using HCS.Umbraco.Passwordless.Otp.Configuration;

namespace HCS.Umbraco.Passwordless.Tests.Otp;

public class OtpOptionsValidatorTests
{
    private readonly OtpOptionsValidator _sut = new();

    [Fact]
    public void Validate_Succeeds_ForValidDefaults()
    {
        var result = _sut.Validate(null, new OtpOptions());
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validate_Succeeds_WhenDisabledAndAllInvalid()
    {
        var opts = new OtpOptions
        {
            Enabled = false,
            CodeLength = 1,
            TokenLifespan = TimeSpan.Zero,
            MaxAttempts = 0
        };
        var result = _sut.Validate(null, opts);
        result.Succeeded.Should().BeTrue("disabled OTP should skip all validation");
    }

    [Theory]
    [InlineData(3)]
    [InlineData(11)]
    public void Validate_Fails_WhenCodeLengthOutOfRange(int length)
    {
        var opts = new OtpOptions { Enabled = true, CodeLength = length };
        var result = _sut.Validate(null, opts);
        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should().Contain("CodeLength");
    }

    [Theory]
    [InlineData(4)]
    [InlineData(6)]
    [InlineData(10)]
    public void Validate_Succeeds_ForValidCodeLength(int length)
    {
        var opts = new OtpOptions { Enabled = true, CodeLength = length };
        var result = _sut.Validate(null, opts);
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validate_Fails_WhenTokenLifespanIsZero()
    {
        var opts = new OtpOptions { Enabled = true, TokenLifespan = TimeSpan.Zero };
        var result = _sut.Validate(null, opts);
        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should().Contain("TokenLifespan");
    }

    [Fact]
    public void Validate_Fails_WhenMaxAttemptsIsZero()
    {
        var opts = new OtpOptions { Enabled = true, MaxAttempts = 0 };
        var result = _sut.Validate(null, opts);
        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should().Contain("MaxAttempts");
    }

    [Fact]
    public void Validate_ReportsAllErrors_WhenMultipleInvalid()
    {
        var opts = new OtpOptions
        {
            Enabled = true,
            CodeLength = 1,
            TokenLifespan = TimeSpan.Zero,
            MaxAttempts = 0
        };
        var result = _sut.Validate(null, opts);
        result.Failures.Should().HaveCount(3);
    }
}
