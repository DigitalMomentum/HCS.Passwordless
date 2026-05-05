using Microsoft.Extensions.DependencyInjection;
using HCS.Umbraco.Passwordless.Otp.Notifications;
using HCS.Umbraco.Passwordless.Otp.Services;

namespace HCS.Umbraco.Passwordless.Otp.DependencyInjection;

public sealed class OtpBuilder
{
    public IServiceCollection Services { get; }

    internal OtpBuilder(IServiceCollection services) => Services = services;

    public OtpBuilder UseNotificationSender<T>() where T : class, IOtpNotificationSender
    {
        Services.AddScoped<IOtpNotificationSender, T>();
        return this;
    }

    public OtpBuilder UseOtpCodeStore<T>() where T : class, IOtpCodeStore
    {
        Services.AddScoped<IOtpCodeStore, T>();
        return this;
    }

    public OtpBuilder UseAttemptCounter<T>() where T : class, IAttemptCounter
    {
        Services.AddScoped<IAttemptCounter, T>();
        return this;
    }
}
