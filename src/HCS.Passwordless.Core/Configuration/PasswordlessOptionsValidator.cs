using Microsoft.Extensions.Options;

namespace HCS.Passwordless.Configuration;

internal sealed class PasswordlessOptionsValidator : IValidateOptions<PasswordlessOptions>
{
    public ValidateOptionsResult Validate(string? name, PasswordlessOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Notifications.FromAddress))
            return ValidateOptionsResult.Fail("Notifications.FromAddress must not be empty.");

        return ValidateOptionsResult.Success;
    }
}
