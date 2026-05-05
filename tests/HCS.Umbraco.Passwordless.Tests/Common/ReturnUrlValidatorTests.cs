using HCS.Umbraco.Passwordless.Endpoints.Shared;

namespace HCS.Umbraco.Passwordless.Tests.Common;

public class ReturnUrlValidatorTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Sanitize_ReturnsDefaultFallback_ForNullOrWhitespace(string? input)
        => ReturnUrlValidator.Sanitize(input).Should().Be("/");

    [Theory]
    [InlineData("//evil.com")]
    [InlineData("//evil.com/path")]
    public void Sanitize_ReturnsDefault_ForProtocolRelativeUrls(string input)
        => ReturnUrlValidator.Sanitize(input).Should().Be("/");

    [Theory]
    [InlineData("http://evil.com")]
    [InlineData("https://evil.com/path")]
    [InlineData("relative/path")]
    [InlineData("javascript:alert(1)")]
    public void Sanitize_ReturnsDefault_ForAbsoluteOrRelativeNoSlash(string input)
        => ReturnUrlValidator.Sanitize(input).Should().Be("/");

    [Theory]
    [InlineData("/")]
    [InlineData("/dashboard")]
    [InlineData("/members/profile")]
    [InlineData("/page?tab=1&foo=bar")]
    [InlineData("/deep/nested/path")]
    public void Sanitize_AllowsLocalRelativeUrls(string input)
        => ReturnUrlValidator.Sanitize(input).Should().Be(input);

    [Fact]
    public void Sanitize_UsesCustomFallback_WhenProvided()
        => ReturnUrlValidator.Sanitize(null, "/custom-fallback").Should().Be("/custom-fallback");

    [Fact]
    public void Sanitize_UsesCustomFallback_ForUnsafeUrl()
        => ReturnUrlValidator.Sanitize("http://evil.com", "/safe").Should().Be("/safe");
}
