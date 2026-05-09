using Microsoft.Extensions.Options;

namespace HCS.Passwordless.Otp.Configuration;

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
        if (options.TokenLifespan > TimeSpan.FromMinutes(30))
            errors.Add("Otp.TokenLifespan must not exceed 30 minutes — longer values extend the brute-force window beyond what the attempt counter can protect.");
        if (options.MaxAttempts <= 0)
            errors.Add("Otp.MaxAttempts must be greater than zero.");
        if (options.LockoutDuration <= TimeSpan.Zero)
            errors.Add("Otp.LockoutDuration must be positive — a zero or negative value disables per-account lockout entirely.");
        if (options.LockoutDuration > TimeSpan.FromDays(1))
            errors.Add("Otp.LockoutDuration must not exceed 24 hours — longer values risk permanently locking out legitimate users.");

        return errors.Count > 0
            ? ValidateOptionsResult.Fail(errors)
            : ValidateOptionsResult.Success;
    }
}
