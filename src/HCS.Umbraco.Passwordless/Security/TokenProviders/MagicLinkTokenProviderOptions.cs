using Microsoft.AspNetCore.Identity;

namespace HCS.Umbraco.Passwordless.Security.TokenProviders;

public sealed class MagicLinkTokenProviderOptions : DataProtectionTokenProviderOptions
{
    public MagicLinkTokenProviderOptions()
    {
        Name = TokenProviderNames.MagicLink;
        TokenLifespan = TimeSpan.FromMinutes(15);
    }
}
