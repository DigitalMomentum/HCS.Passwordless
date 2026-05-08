using Fido2NetLib;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using HCS.Umbraco.Passwordless.Auth;
using HCS.Umbraco.Passwordless.DependencyInjection;
using HCS.Umbraco.Passwordless.WebAuthn.Auth;
using HCS.Umbraco.Passwordless.WebAuthn.Configuration;
using HCS.Umbraco.Passwordless.WebAuthn.Services;
using HCS.Umbraco.Passwordless.WebAuthn.Storage;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Infrastructure.Scoping;

namespace HCS.Umbraco.Passwordless.WebAuthn.DependencyInjection;

public static class WebAuthnBuilderExtensions
{
    public static IUmbracoBuilder AddPasswordlessWebAuthn(
        this IUmbracoBuilder builder,
        Action<WebAuthnBuilder>? configure = null)
    {
        var services = builder.Services;

        // Shared core infrastructure (idempotent)
        services.AddPasswordlessCoreOnce();

        services.AddOptions<WebAuthnOptions>()
            .BindConfiguration(WebAuthnOptions.SectionName);
        services.AddSingleton<IValidateOptions<WebAuthnOptions>, WebAuthnOptionsValidator>();

        services.TryAddScoped<IMemberCredentialStore>(sp =>
            new UmbracoDbMemberCredentialStore(sp.GetRequiredService<IScopeProvider>()));
        services.TryAddScoped<IWebAuthnChallengeStore, DistributedCacheChallengeStore>();

        services.AddOptions<Fido2Configuration>().Configure<IOptions<WebAuthnOptions>>((fido, opts) =>
        {
            var wa = opts.Value;
            fido.ServerDomain = wa.RpId ?? "localhost";
            fido.ServerName = wa.RpName;
            fido.Origins = wa.Origins;
            fido.TimestampDriftTolerance = 300_000;
        });
        services.TryAddSingleton<IFido2>(sp =>
        {
            var fido2Opts = sp.GetRequiredService<IOptions<Fido2Configuration>>().Value;
            return new Fido2(fido2Opts);
        });

        services.TryAddEnumerable(
            ServiceDescriptor.Scoped<IPasswordlessAuthFactor, WebAuthnAuthFactor>());

        var webAuthnBuilder = new WebAuthnBuilder(services);
        configure?.Invoke(webAuthnBuilder);

        return builder;
    }
}
