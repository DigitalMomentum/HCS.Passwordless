using Microsoft.Extensions.Options;
using HCS.Umbraco.Passwordless.Auth;
using HCS.Umbraco.Passwordless.WebAuthn.Configuration;

namespace HCS.Umbraco.Passwordless.WebAuthn.Auth;

internal sealed class WebAuthnAuthFactor : IPasswordlessAuthFactor
{
    private readonly IOptionsMonitor<WebAuthnOptions> _opts;

    public WebAuthnAuthFactor(IOptionsMonitor<WebAuthnOptions> opts) => _opts = opts;

    public string Name => "webauthn";
    public bool IsEnabled => _opts.CurrentValue.Enabled;
}
