using Microsoft.Extensions.Options;

namespace HCS.Umbraco.Passwordless.Otp.Configuration;

internal sealed class OtpOptionsValidator : IValidateOptions<OtpOptions>
{
    public ValidateOptionsResult Validate(string? name, OtpOptions options)
    {
        if (!options.Enabled) return ValidateOptionsResult.Success;

        var errors = new List<string>();

        if (options.CodeLength is < 4 or > 10)
            errors.Add("Otp.CodeLength must be between 4 and 10.");
        if (options.TokenLifespan <= TimeSpan.Zero)
            errors.Add("Otp.TokenLifespan must be positive.");
        if (options.MaxAttempts <= 0)
            errors.Add("Otp.MaxAttempts must be greater than zero.");

        return errors.Count > 0
            ? ValidateOptionsResult.Fail(errors)
            : ValidateOptionsResult.Success;
    }
}
