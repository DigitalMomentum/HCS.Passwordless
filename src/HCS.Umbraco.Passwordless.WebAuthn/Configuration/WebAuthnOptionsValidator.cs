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

        return errors.Count > 0
            ? ValidateOptionsResult.Fail(errors)
            : ValidateOptionsResult.Success;
    }
}
