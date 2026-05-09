using HCS.Passwordless.Security.TokenProviders;

namespace HCS.Passwordless.Otp.Security.TokenProviders;

public sealed class OtpTokenProviderOptions
{
    public string Name { get; set; } = TokenProviderNames.Otp;
    public TimeSpan TokenLifespan { get; set; } = TimeSpan.FromMinutes(5);
    public int CodeLength { get; set; } = 6;
}
