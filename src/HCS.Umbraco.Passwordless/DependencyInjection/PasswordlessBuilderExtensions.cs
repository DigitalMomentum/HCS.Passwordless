using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using HCS.Umbraco.Passwordless.Auth;
using HCS.Umbraco.Passwordless.Configuration;
using HCS.Umbraco.Passwordless.Notifications;
using HCS.Umbraco.Passwordless.RateLimiting;
using HCS.Umbraco.Passwordless.Security.TokenProviders;
using HCS.Umbraco.Passwordless.Services;
using Umbraco.Cms.Core.DependencyInjection;

namespace HCS.Umbraco.Passwordless.DependencyInjection;

public static class PasswordlessBuilderExtensions
{
    public static IUmbracoBuilder AddPasswordlessMembers(
        this IUmbracoBuilder builder,
        Action<PasswordlessBuilder>? configure = null)
    {
        var services = builder.Services;

        services.AddOptions<PasswordlessOptions>()
            .BindConfiguration(PasswordlessOptions.SectionName);
        services.AddSingleton<IValidateOptions<PasswordlessOptions>, PasswordlessOptionsValidator>();

        // Cache infrastructure (safe double-add)
        services.AddMemoryCache();
        services.TryAddSingleton<Microsoft.Extensions.Caching.Distributed.IDistributedCache,
            Microsoft.Extensions.Caching.Distributed.MemoryDistributedCache>();

        // Core services
        services.TryAddScoped<IPasswordlessNotificationSender, EmailNotificationSender>();
        services.TryAddScoped<IRazorViewRenderer, RazorViewRenderer>();
        services.TryAddScoped<ISingleUseTokenStore, DistributedCacheSingleUseTokenStore>();
        services.TryAddSingleton<IPasswordlessRateLimiter, SlidingWindowRateLimiter>();
        services.TryAddScoped<IMemberLookupService, MemberLookupService>();
        services.TryAddScoped<IPasswordlessSignInService, PasswordlessSignInService>();
        services.TryAddSingleton<IPasswordlessClock, SystemClock>();

        // Magic link token provider
        services.TryAddScoped<MagicLinkTokenProvider>();

        services.Configure<IdentityOptions>(o =>
        {
            o.Tokens.ProviderMap[TokenProviderNames.MagicLink] =
                new TokenProviderDescriptor(typeof(MagicLinkTokenProvider));
        });

        services.AddOptions<MagicLinkTokenProviderOptions>()
            .Configure<IOptions<PasswordlessOptions>>((p, opts) =>
            {
                p.TokenLifespan = opts.Value.MagicLink.TokenLifespan;
            });

        // Auth factor registration (allows add-ons to report their enabled state)
        services.TryAddEnumerable(
            ServiceDescriptor.Scoped<IPasswordlessAuthFactor, MagicLinkAuthFactor>());

        var passwordlessBuilder = new PasswordlessBuilder(services);
        configure?.Invoke(passwordlessBuilder);

        return builder;
    }
}
