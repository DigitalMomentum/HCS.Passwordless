using Microsoft.Extensions.Options;
using HCS.Umbraco.Passwordless.Auth;
using HCS.Umbraco.Passwordless.Otp.Configuration;

namespace HCS.Umbraco.Passwordless.Otp.Auth;

internal sealed class OtpAuthFactor : IPasswordlessAuthFactor
{
    private readonly IOptionsMonitor<OtpOptions> _opts;

    public OtpAuthFactor(IOptionsMonitor<OtpOptions> opts) => _opts = opts;

    public string Name => "otp";
    public bool IsEnabled => _opts.CurrentValue.Enabled;
}
