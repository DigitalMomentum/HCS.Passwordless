using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using HCS.Umbraco.Passwordless.Auth;
using HCS.Umbraco.Passwordless.Configuration;
using HCS.Umbraco.Passwordless.Notifications;
using HCS.Umbraco.Passwordless.Security.TokenProviders;
using Umbraco.Cms.Core.DependencyInjection;

namespace HCS.Umbraco.Passwordless.DependencyInjection;

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
