using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using HCS.Umbraco.Passwordless.Configuration;
using HCS.Umbraco.Passwordless.Notifications;
using HCS.Umbraco.Passwordless.RateLimiting;
using HCS.Umbraco.Passwordless.Services;
using Microsoft.Extensions.Hosting;

namespace HCS.Umbraco.Passwordless.DependencyInjection;

public static class PasswordlessCoreServiceCollectionExtensions
{
    private sealed class CoreServicesMarker { }

    /// <summary>
    /// Registers shared passwordless infrastructure. Called automatically by AddPasswordlessMembers,
    /// AddPasswordlessOtp, and AddPasswordlessWebAuthn — safe to call multiple times.
    /// </summary>
    public static IServiceCollection AddPasswordlessCoreOnce(this IServiceCollection services)
    {
        if (services.Any(d => d.ServiceType == typeof(CoreServicesMarker)))
            return services;

        services.AddSingleton<CoreServicesMarker>();

        services.AddOptions<PasswordlessOptions>()
            .BindConfiguration(PasswordlessOptions.SectionName);
        services.AddSingleton<IValidateOptions<PasswordlessOptions>, PasswordlessOptionsValidator>();

        services.AddMemoryCache();

        services.TryAddScoped<IRazorViewRenderer, RazorViewRenderer>();
        services.TryAddSingleton<ISingleUseTokenStore, InMemorySingleUseTokenStore>();
        services.TryAddSingleton<IPasswordlessRateLimiter, FixedWindowRateLimiter>();
        services.TryAddScoped<IMemberLookupService, MemberLookupService>();
        services.TryAddScoped<IPasswordlessSignInService, PasswordlessSignInService>();
        services.TryAddSingleton<IPasswordlessClock, SystemClock>();

        services.AddHostedService<DistributedCacheStartupCheck>();

        return services;
    }
}
