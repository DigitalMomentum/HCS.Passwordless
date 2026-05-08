using Microsoft.Extensions.Options;

namespace HCS.Umbraco.Passwordless.Configuration;

internal sealed class MagicLinkOptionsValidator : IValidateOptions<MagicLinkOptions>
{
    public ValidateOptionsResult Validate(string? name, MagicLinkOptions options)
    {
        if (options.Enabled && options.TokenLifespan <= TimeSpan.Zero)
            return ValidateOptionsResult.Fail("MagicLink.TokenLifespan must be positive.");

        return ValidateOptionsResult.Success;
    }
}
