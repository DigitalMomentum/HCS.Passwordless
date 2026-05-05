using Microsoft.Extensions.DependencyInjection;
using HCS.Umbraco.Passwordless.WebAuthn.Services;
using HCS.Umbraco.Passwordless.WebAuthn.Storage;

namespace HCS.Umbraco.Passwordless.WebAuthn.DependencyInjection;

public sealed class WebAuthnBuilder
{
    public IServiceCollection Services { get; }

    internal WebAuthnBuilder(IServiceCollection services) => Services = services;

    public WebAuthnBuilder UseMemberCredentialStore<T>() where T : class, IMemberCredentialStore
    {
        Services.AddScoped<IMemberCredentialStore, T>();
        return this;
    }

    public WebAuthnBuilder UseChallengeStore<T>() where T : class, IWebAuthnChallengeStore
    {
        Services.AddScoped<IWebAuthnChallengeStore, T>();
        return this;
    }
}
