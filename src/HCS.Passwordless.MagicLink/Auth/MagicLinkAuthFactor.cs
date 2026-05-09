using Microsoft.Extensions.Options;
using HCS.Passwordless.Auth;
using HCS.Passwordless.MagicLink.Configuration;

namespace HCS.Passwordless.MagicLink.Auth;

internal sealed class MagicLinkAuthFactor : IPasswordlessAuthFactor
{
    private readonly IOptionsMonitor<MagicLinkOptions> _opts;

    public MagicLinkAuthFactor(IOptionsMonitor<MagicLinkOptions> opts) => _opts = opts;

    public string Name => "magic-link";
    public bool IsEnabled => _opts.CurrentValue.Enabled;
}
