namespace HCS.Umbraco.Passwordless.Security.TokenProviders;

internal static class TokenProviderNames
{
    public const string MagicLink = "passwordless-magic-link";
    public const string Otp = "passwordless-otp";
    public const string PurposeMagicLinkLogin = "magic-link:login";
    public const string PurposeOtpLogin = "otp:login";
}
