using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using HCS.Passwordless.Auth;
using HCS.Passwordless.DependencyInjection;
using HCS.Passwordless.Otp.Auth;
using HCS.Passwordless.Otp.Configuration;
using HCS.Passwordless.Otp.Notifications;
using HCS.Passwordless.Otp.Security.TokenProviders;
using HCS.Passwordless.Otp.Services;
using HCS.Passwordless.Security.TokenProviders;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Security;

namespace HCS.Passwordless.Otp.DependencyInjection;

public static class OtpBuilderExtensions
{
    public static IUmbracoBuilder AddPasswordlessOtp(
        this IUmbracoBuilder builder,
        Action<OtpBuilder>? configure = null)
    {
        var services = builder.Services;

        // Shared core infrastructure (idempotent)
        services.AddPasswordlessCoreOnce();

        services.AddOptions<OtpOptions>()
            .BindConfiguration(OtpOptions.SectionName);
        services.AddSingleton<IValidateOptions<OtpOptions>, OtpOptionsValidator>();

        services.TryAddScoped<IOtpNotificationSender, EmailOtpNotificationSender>();
        services.TryAddScoped<IOtpCodeStore, DistributedCacheOtpCodeStore>();
        services.TryAddSingleton<IAttemptCounter, InMemoryAttemptCounter>();
        services.TryAddScoped<OtpTokenProvider>();

        services.Configure<IdentityOptions>(o =>
        {
            o.Tokens.ProviderMap[TokenProviderNames.Otp] =
                new TokenProviderDescriptor(typeof(OtpTokenProvider));
        });

        services.AddOptions<OtpTokenProviderOptions>()
            .Configure<IOptions<OtpOptions>>((p, opts) =>
            {
                p.TokenLifespan = opts.Value.TokenLifespan;
                p.CodeLength = opts.Value.CodeLength;
            });

        services.TryAddEnumerable(
            ServiceDescriptor.Scoped<IPasswordlessAuthFactor, OtpAuthFactor>());

        var otpBuilder = new OtpBuilder(services);
        configure?.Invoke(otpBuilder);

        return builder;
    }
}
