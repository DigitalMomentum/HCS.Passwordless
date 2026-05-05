using Microsoft.Extensions.Options;

namespace HCS.Umbraco.Passwordless.Configuration;

internal sealed class PasswordlessOptionsValidator : IValidateOptions<PasswordlessOptions>
{
    public ValidateOptionsResult Validate(string? name, PasswordlessOptions options)
    {
        var errors = new List<string>();

        if (options.MagicLink.Enabled && options.MagicLink.TokenLifespan <= TimeSpan.Zero)
            errors.Add("MagicLink.TokenLifespan must be positive.");

        if (string.IsNullOrWhiteSpace(options.Notifications.FromAddress))
            errors.Add("Notifications.FromAddress must not be empty.");

        return errors.Count > 0
            ? ValidateOptionsResult.Fail(errors)
            : ValidateOptionsResult.Success;
    }
}
