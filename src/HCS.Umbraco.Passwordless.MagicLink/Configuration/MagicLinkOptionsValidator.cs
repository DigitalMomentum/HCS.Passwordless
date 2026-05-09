using Microsoft.Extensions.Options;

namespace HCS.Umbraco.Passwordless.MagicLink.Configuration;

internal sealed class MagicLinkOptionsValidator : IValidateOptions<MagicLinkOptions>
{
    public ValidateOptionsResult Validate(string? name, MagicLinkOptions options)
    {
        if (!options.Enabled) return ValidateOptionsResult.Success;

        var errors = new List<string>();

        if (options.TokenLifespan <= TimeSpan.Zero)
            errors.Add("MagicLink.TokenLifespan must be positive.");

        if (options.TokenLifespan > TimeSpan.FromHours(1))
            errors.Add("MagicLink.TokenLifespan must not exceed 1 hour — longer values produce long-lived credentials.");

        if (!options.SingleUse)
            errors.Add("MagicLink.SingleUse must be true. Disabling single-use enforcement allows token replay for the entire TokenLifespan.");

        return errors.Count > 0
            ? ValidateOptionsResult.Fail(errors)
            : ValidateOptionsResult.Success;
    }
}
