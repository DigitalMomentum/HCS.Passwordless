using Microsoft.Extensions.DependencyInjection;
using HCS.Umbraco.Passwordless.Configuration;
using HCS.Umbraco.Passwordless.Notifications;
using HCS.Umbraco.Passwordless.RateLimiting;
using HCS.Umbraco.Passwordless.Services;

namespace HCS.Umbraco.Passwordless.DependencyInjection;

public sealed class PasswordlessBuilder
{
    public IServiceCollection Services { get; }

    internal PasswordlessBuilder(IServiceCollection services) => Services = services;

    public PasswordlessBuilder Configure(Action<PasswordlessOptions> configure)
    {
        Services.Configure(configure);
        return this;
    }

    public PasswordlessBuilder UseNotificationSender<T>() where T : class, IPasswordlessNotificationSender
    {
        Services.AddScoped<IPasswordlessNotificationSender, T>();
        return this;
    }

    public PasswordlessBuilder UseSingleUseTokenStore<T>() where T : class, ISingleUseTokenStore
    {
        Services.AddScoped<ISingleUseTokenStore, T>();
        return this;
    }

    public PasswordlessBuilder UseRateLimiter<T>() where T : class, IPasswordlessRateLimiter
    {
        Services.AddSingleton<IPasswordlessRateLimiter, T>();
        return this;
    }
}
