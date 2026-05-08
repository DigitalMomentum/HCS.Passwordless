using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using HCS.Umbraco.Passwordless.Configuration;
using HCS.Umbraco.Passwordless.Notifications;
using HCS.Umbraco.Passwordless.RateLimiting;
using HCS.Umbraco.Passwordless.Services;

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
        services.TryAddSingleton<IDistributedCache, MemoryDistributedCache>();

        services.TryAddScoped<IRazorViewRenderer, RazorViewRenderer>();
        services.TryAddScoped<ISingleUseTokenStore, DistributedCacheSingleUseTokenStore>();
        services.TryAddSingleton<IPasswordlessRateLimiter, SlidingWindowRateLimiter>();
        services.TryAddScoped<IMemberLookupService, MemberLookupService>();
        services.TryAddScoped<IPasswordlessSignInService, PasswordlessSignInService>();
        services.TryAddSingleton<IPasswordlessClock, SystemClock>();

        return services;
    }
}
