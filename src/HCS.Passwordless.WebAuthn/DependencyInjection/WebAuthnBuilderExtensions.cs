using Fido2NetLib;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using HCS.Passwordless.Auth;
using HCS.Passwordless.DependencyInjection;
using HCS.Passwordless.WebAuthn.Auth;
using HCS.Passwordless.WebAuthn.Configuration;
using HCS.Passwordless.WebAuthn.Services;
using HCS.Passwordless.WebAuthn.Storage;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Infrastructure.Scoping;

namespace HCS.Passwordless.WebAuthn.DependencyInjection;

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
            fido.TimestampDriftTolerance = wa.TimestampDriftToleranceMs;
        });
        // IFido2 is a singleton that captures WebAuthnOptions (Origins, RpId) at first resolution.
        // Runtime config reloads are intentionally not picked up — origin pinning must not change
        // without a restart, and accepting a stale binding is safer than accepting an attacker-
        // supplied origin mid-flight. Document this in the configuration guide.
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
