using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace HCS.Umbraco.Passwordless.WebAuthn.Configuration;

internal sealed class WebAuthnOptionsValidator : IValidateOptions<WebAuthnOptions>
{
    private readonly IHostEnvironment _environment;

    public WebAuthnOptionsValidator(IHostEnvironment environment) => _environment = environment;

    public ValidateOptionsResult Validate(string? name, WebAuthnOptions options)
    {
        if (!options.Enabled) return ValidateOptionsResult.Success;

        var errors = new List<string>();

        if (_environment.IsProduction() && options.Origins.Count == 0)
            errors.Add("WebAuthn.Origins must not be empty in Production when WebAuthn is enabled.");

        if (string.IsNullOrEmpty(options.DecoyHmacKey))
            errors.Add("WebAuthn.DecoyHmacKey must be set to a random secret value.");
        else if (Encoding.UTF8.GetByteCount(options.DecoyHmacKey) < 16)
            errors.Add("WebAuthn.DecoyHmacKey must be at least 16 bytes (UTF-8 encoded).");

        return errors.Count > 0
            ? ValidateOptionsResult.Fail(errors)
            : ValidateOptionsResult.Success;
    }
}
