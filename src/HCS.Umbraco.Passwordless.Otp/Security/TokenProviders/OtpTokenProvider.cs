using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using HCS.Umbraco.Passwordless.Otp.Services;
using HCS.Umbraco.Passwordless.Security;
using Umbraco.Cms.Core.Security;

namespace HCS.Umbraco.Passwordless.Otp.Security.TokenProviders;

public sealed class OtpTokenProvider : IUserTwoFactorTokenProvider<MemberIdentityUser>
{
    private readonly IOtpCodeStore _store;
    private readonly IOptions<OtpTokenProviderOptions> _options;

    public OtpTokenProvider(IOtpCodeStore store, IOptions<OtpTokenProviderOptions> options)
    {
        _store = store;
        _options = options;
    }

    public Task<bool> CanGenerateTwoFactorTokenAsync(
        UserManager<MemberIdentityUser> manager, MemberIdentityUser user)
        => Task.FromResult(!string.IsNullOrEmpty(user.Email));

    public async Task<string> GenerateAsync(
        string purpose, UserManager<MemberIdentityUser> manager, MemberIdentityUser user)
    {
        var opts = _options.Value;
        var code = GenerateNumericCode(opts.CodeLength);
        var hash = ComputeHash(code, user.Id);
        await _store.SetAsync(user.Id, purpose, hash, opts.TokenLifespan);
        return code;
    }

    public async Task<bool> ValidateAsync(
        string purpose, string token, UserManager<MemberIdentityUser> manager, MemberIdentityUser user)
    {
        var stored = await _store.GetAsync(user.Id, purpose);
        if (stored is null) return false;

        var candidate = ComputeHash(token, user.Id);
        var ok = ConstantTime.Equals(candidate, stored);

        if (ok) await _store.DeleteAsync(user.Id, purpose);
        return ok;
    }

    private static string GenerateNumericCode(int length)
    {
        var max = (int)Math.Pow(10, length);
        var value = RandomNumberGenerator.GetInt32(0, max);
        return value.ToString().PadLeft(length, '0');
    }

    private static byte[] ComputeHash(string code, string memberId)
    {
        var key = Encoding.UTF8.GetBytes(memberId.Length > 0 ? memberId : "unknown");
        return HMACSHA256.HashData(key, Encoding.UTF8.GetBytes(code));
    }
}
