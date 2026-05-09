using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using HCS.Passwordless.Auth;
using HCS.Passwordless.DependencyInjection;
using HCS.Passwordless.Notifications;
using HCS.Passwordless.Security.TokenProviders;
using HCS.Passwordless.MagicLink.Auth;
using HCS.Passwordless.MagicLink.Configuration;
using HCS.Passwordless.MagicLink.Notifications;
using HCS.Passwordless.MagicLink.Security.TokenProviders;
using Umbraco.Cms.Core.DependencyInjection;

namespace HCS.Passwordless.MagicLink.DependencyInjection;

public static class MagicLinkBuilderExtensions
{
    public static IUmbracoBuilder AddPasswordlessMagicLink(
        this IUmbracoBuilder builder,
        Action<MagicLinkBuilder>? configure = null)
    {
        var services = builder.Services;

        // Shared core infrastructure (idempotent)
        services.AddPasswordlessCoreOnce();

        // Magic link options
        services.AddOptions<MagicLinkOptions>()
            .BindConfiguration(MagicLinkOptions.SectionName);
        services.AddSingleton<IValidateOptions<MagicLinkOptions>, MagicLinkOptionsValidator>();

        // Magic link services
        services.TryAddScoped<IPasswordlessNotificationSender, EmailNotificationSender>();
        services.TryAddScoped<MagicLinkTokenProvider>();

        services.Configure<IdentityOptions>(o =>
        {
            o.Tokens.ProviderMap[TokenProviderNames.MagicLink] =
                new TokenProviderDescriptor(typeof(MagicLinkTokenProvider));
        });

        services.AddOptions<MagicLinkTokenProviderOptions>()
            .Configure<IOptions<MagicLinkOptions>>((p, opts) =>
            {
                p.TokenLifespan = opts.Value.TokenLifespan;
            });

        services.TryAddEnumerable(
            ServiceDescriptor.Scoped<IPasswordlessAuthFactor, MagicLinkAuthFactor>());

        var magicLinkBuilder = new MagicLinkBuilder(services);
        configure?.Invoke(magicLinkBuilder);

        return builder;
    }
}
