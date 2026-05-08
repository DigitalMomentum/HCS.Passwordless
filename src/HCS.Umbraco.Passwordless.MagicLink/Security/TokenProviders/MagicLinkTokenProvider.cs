using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Security;

namespace HCS.Umbraco.Passwordless.MagicLink.Security.TokenProviders;

public sealed class MagicLinkTokenProvider : DataProtectorTokenProvider<MemberIdentityUser>
{
    public MagicLinkTokenProvider(
        IDataProtectionProvider dataProtectionProvider,
        IOptions<MagicLinkTokenProviderOptions> options,
        ILogger<MagicLinkTokenProvider> logger)
        : base(dataProtectionProvider, options, logger)
    {
    }
}
