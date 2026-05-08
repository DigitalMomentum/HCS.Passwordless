using Microsoft.Extensions.DependencyInjection;
using HCS.Umbraco.Passwordless.Configuration;
using HCS.Umbraco.Passwordless.Notifications;
using HCS.Umbraco.Passwordless.RateLimiting;
using HCS.Umbraco.Passwordless.Services;

namespace HCS.Umbraco.Passwordless.MagicLink.DependencyInjection;

public sealed class MagicLinkBuilder
{
    public IServiceCollection Services { get; }

    internal MagicLinkBuilder(IServiceCollection services) => Services = services;

    public MagicLinkBuilder Configure(Action<PasswordlessOptions> configure)
    {
        Services.Configure(configure);
        return this;
    }

    public MagicLinkBuilder UseNotificationSender<T>() where T : class, IPasswordlessNotificationSender
    {
        Services.AddScoped<IPasswordlessNotificationSender, T>();
        return this;
    }

    public MagicLinkBuilder UseSingleUseTokenStore<T>() where T : class, ISingleUseTokenStore
    {
        Services.AddScoped<ISingleUseTokenStore, T>();
        return this;
    }

    public MagicLinkBuilder UseRateLimiter<T>() where T : class, IPasswordlessRateLimiter
    {
        Services.AddSingleton<IPasswordlessRateLimiter, T>();
        return this;
    }
}
