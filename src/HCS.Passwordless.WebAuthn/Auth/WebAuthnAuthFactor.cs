using Microsoft.Extensions.Options;
using HCS.Passwordless.Auth;
using HCS.Passwordless.WebAuthn.Configuration;

namespace HCS.Passwordless.WebAuthn.Auth;

internal sealed class WebAuthnAuthFactor : IPasswordlessAuthFactor
{
    private readonly IOptionsMonitor<WebAuthnOptions> _opts;

    public WebAuthnAuthFactor(IOptionsMonitor<WebAuthnOptions> opts) => _opts = opts;

    public string Name => "webauthn";
    public bool IsEnabled => _opts.CurrentValue.Enabled;
}
