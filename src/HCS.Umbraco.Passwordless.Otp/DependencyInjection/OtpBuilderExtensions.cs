using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using HCS.Umbraco.Passwordless.Auth;
using HCS.Umbraco.Passwordless.Otp.Auth;
using HCS.Umbraco.Passwordless.Otp.Configuration;
using HCS.Umbraco.Passwordless.Otp.Notifications;
using HCS.Umbraco.Passwordless.Otp.Security.TokenProviders;
using HCS.Umbraco.Passwordless.Otp.Services;
using HCS.Umbraco.Passwordless.Security.TokenProviders;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Security;

namespace HCS.Umbraco.Passwordless.Otp.DependencyInjection;

public static class OtpBuilderExtensions
{
    public static IUmbracoBuilder AddPasswordlessOtp(
        this IUmbracoBuilder builder,
        Action<OtpBuilder>? configure = null)
    {
        var services = builder.Services;

        services.AddOptions<OtpOptions>()
            .BindConfiguration(OtpOptions.SectionName);
        services.AddSingleton<IValidateOptions<OtpOptions>, OtpOptionsValidator>();

        services.TryAddScoped<IOtpNotificationSender, EmailOtpNotificationSender>();
        services.TryAddScoped<IOtpCodeStore, DistributedCacheOtpCodeStore>();
        services.TryAddScoped<IAttemptCounter, DistributedCacheAttemptCounter>();
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
