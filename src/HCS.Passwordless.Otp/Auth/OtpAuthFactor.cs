using Microsoft.Extensions.Options;
using HCS.Passwordless.Auth;
using HCS.Passwordless.Otp.Configuration;

namespace HCS.Passwordless.Otp.Auth;

internal sealed class OtpAuthFactor : IPasswordlessAuthFactor
{
    private readonly IOptionsMonitor<OtpOptions> _opts;

    public OtpAuthFactor(IOptionsMonitor<OtpOptions> opts) => _opts = opts;

    public string Name => "otp";
    public bool IsEnabled => _opts.CurrentValue.Enabled;
}
