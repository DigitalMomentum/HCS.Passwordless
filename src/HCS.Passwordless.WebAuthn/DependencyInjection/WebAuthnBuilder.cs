using Microsoft.Extensions.DependencyInjection;
using HCS.Passwordless.WebAuthn.Services;
using HCS.Passwordless.WebAuthn.Storage;

namespace HCS.Passwordless.WebAuthn.DependencyInjection;

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
