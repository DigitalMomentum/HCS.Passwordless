using HCS.Umbraco.Passwordless.Otp.Security.TokenProviders;
using HCS.Umbraco.Passwordless.Otp.Services;
using Umbraco.Cms.Core.Security;

namespace HCS.Umbraco.Passwordless.Tests.Otp;

public class OtpTokenProviderTests
{
    private readonly IOtpCodeStore _store = Substitute.For<IOtpCodeStore>();

    private OtpTokenProvider CreateProvider(int codeLength = 6)
        => new(_store, Options.Create(new OtpTokenProviderOptions
        {
            CodeLength = codeLength,
            TokenLifespan = TimeSpan.FromMinutes(5)
        }));

    private static MemberIdentityUser Member(string id = "m1", string email = "user@example.com")
        => new() { Id = id, Email = email, SecurityStamp = "stamp-value" };

    [Fact]
    public async Task CanGenerateTwoFactorTokenAsync_ReturnsTrueWhenEmailPresent()
    {
        var result = await CreateProvider().CanGenerateTwoFactorTokenAsync(null!, Member());
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanGenerateTwoFactorTokenAsync_ReturnsFalseWhenEmailAbsent()
    {
        var member = new MemberIdentityUser { Id = "no-email", Email = null };
        var result = await CreateProvider().CanGenerateTwoFactorTokenAsync(null!, member);
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(4)]
    [InlineData(6)]
    [InlineData(8)]
    [InlineData(10)]
    public async Task GenerateAsync_ReturnsNumericCodeOfCorrectLength(int length)
    {
        var provider = CreateProvider(length);
        var code = await provider.GenerateAsync("otp:login", null!, Member());

        code.Should().HaveLength(length).And.MatchRegex(@"^\d+$");
    }

    [Fact]
    public async Task GenerateAsync_StoresHashInCodeStore()
    {
        var provider = CreateProvider();
        var member = Member();

        await provider.GenerateAsync("otp:login", null!, member);

        await _store.Received(1).SetAsync(
            member.Id,
            "otp:login",
            Arg.Any<byte[]>(),
            TimeSpan.FromMinutes(5));
    }

    [Fact]
    public async Task ValidateAsync_ReturnsTrueForCorrectCode()
    {
        var member = Member();
        byte[]? storedHash = null;

        _store.When(s => s.SetAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<byte[]>(), Arg.Any<TimeSpan>()))
            .Do(ci => storedHash = ci.ArgAt<byte[]>(2));

        var provider = CreateProvider();
        var code = await provider.GenerateAsync("otp:login", null!, member);

        _store.GetAsync(member.Id, "otp:login").Returns(storedHash);

        var valid = await provider.ValidateAsync("otp:login", code, null!, member);
        valid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_ReturnsFalseForWrongCode()
    {
        var member = Member();
        byte[]? storedHash = null;

        _store.When(s => s.SetAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<byte[]>(), Arg.Any<TimeSpan>()))
            .Do(ci => storedHash = ci.ArgAt<byte[]>(2));

        var provider = CreateProvider();
        await provider.GenerateAsync("otp:login", null!, member);
        _store.GetAsync(member.Id, "otp:login").Returns(storedHash);

        var valid = await provider.ValidateAsync("otp:login", "000000", null!, member);
        valid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateAsync_ReturnsFalseWhenNoStoredCode()
    {
        _store.GetAsync(Arg.Any<string>(), Arg.Any<string>()).Returns((byte[]?)null);
        var valid = await CreateProvider().ValidateAsync("otp:login", "123456", null!, Member());
        valid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateAsync_DeletesCodeAfterSuccessfulValidation()
    {
        var member = Member();
        byte[]? storedHash = null;

        _store.When(s => s.SetAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<byte[]>(), Arg.Any<TimeSpan>()))
            .Do(ci => storedHash = ci.ArgAt<byte[]>(2));

        var provider = CreateProvider();
        var code = await provider.GenerateAsync("otp:login", null!, member);
        _store.GetAsync(member.Id, "otp:login").Returns(storedHash);

        await provider.ValidateAsync("otp:login", code, null!, member);

        await _store.Received(1).DeleteAsync(member.Id, "otp:login");
    }

    [Fact]
    public async Task ValidateAsync_DoesNotDeleteOnFailure()
    {
        _store.GetAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(new byte[] { 1, 2, 3 });

        await CreateProvider().ValidateAsync("otp:login", "wrong", null!, Member());

        await _store.DidNotReceive().DeleteAsync(Arg.Any<string>(), Arg.Any<string>());
    }
}
