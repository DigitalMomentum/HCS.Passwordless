using HCS.Passwordless.Endpoints.Shared;

namespace HCS.Passwordless.Tests.Common;

public class Sha256HelperTests
{
    [Fact]
    public void Hash_ReturnsLowercaseHex64Chars()
    {
        var result = Sha256Helper.Hash("hello");
        result.Should().HaveLength(64).And.MatchRegex("^[0-9a-f]+$");
    }

    [Fact]
    public void Hash_IsDeterministic()
        => Sha256Helper.Hash("test-input").Should().Be(Sha256Helper.Hash("test-input"));

    [Fact]
    public void Hash_DifferentInputsProduceDifferentHashes()
        => Sha256Helper.Hash("foo").Should().NotBe(Sha256Helper.Hash("bar"));

    [Fact]
    public void Hash_MatchesKnownSha256Value()
    {
        // SHA-256("hello") is a well-known constant
        Sha256Helper.Hash("hello")
            .Should().Be("2cf24dba5fb0a30e26e83b2ac5b9e29e1b161e5c1fa7425e73043362938b9824");
    }

    [Fact]
    public void Hash_EmptyStringProducesKnownValue()
    {
        // SHA-256("") is also a well-known constant
        Sha256Helper.Hash(string.Empty)
            .Should().Be("e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855");
    }

    [Theory]
    [InlineData("user@example.com", "USER@EXAMPLE.COM")]
    [InlineData("user@example.com", "User@Example.Com")]
    [InlineData("user@example.com", "  user@example.com  ")]
    [InlineData("user@example.com", "  USER@EXAMPLE.COM  ")]
    public void Hash_NormalisedEmailVariantsProduceSameKey(string a, string b)
    {
        // Rate-limit keys are derived from Trim().ToLowerInvariant() — case/whitespace
        // variants of the same address must hash identically so the limit cannot be bypassed.
        Sha256Helper.Hash(a.Trim().ToLowerInvariant())
            .Should().Be(Sha256Helper.Hash(b.Trim().ToLowerInvariant()));
    }
}
