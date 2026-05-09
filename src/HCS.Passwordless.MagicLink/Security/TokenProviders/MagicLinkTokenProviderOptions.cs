using HCS.Passwordless.Security.TokenProviders;
using Microsoft.AspNetCore.Identity;

namespace HCS.Passwordless.MagicLink.Security.TokenProviders;

public sealed class MagicLinkTokenProviderOptions : DataProtectionTokenProviderOptions
{
    public MagicLinkTokenProviderOptions()
    {
        Name = TokenProviderNames.MagicLink;
        TokenLifespan = TimeSpan.FromMinutes(15);
    }
}
