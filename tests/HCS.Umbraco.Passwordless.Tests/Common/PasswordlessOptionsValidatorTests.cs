using HCS.Umbraco.Passwordless.Configuration;

namespace HCS.Umbraco.Passwordless.Tests.Common;

public class PasswordlessOptionsValidatorTests
{
    private readonly PasswordlessOptionsValidator _sut = new();

    [Fact]
    public void Validate_Succeeds_ForValidDefaults()
    {
        var result = _sut.Validate(null, new PasswordlessOptions());
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validate_Fails_WhenFromAddressIsEmpty()
    {
        var opts = new PasswordlessOptions
        {
            Notifications = new NotificationOptions { FromAddress = string.Empty }
        };
        var result = _sut.Validate(null, opts);
        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should().Contain("FromAddress");
    }

    [Fact]
    public void Validate_Fails_WhenFromAddressIsWhitespace()
    {
        var opts = new PasswordlessOptions
        {
            Notifications = new NotificationOptions { FromAddress = "   " }
        };
        var result = _sut.Validate(null, opts);
        result.Succeeded.Should().BeFalse();
    }
}
