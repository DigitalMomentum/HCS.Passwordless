using Microsoft.Extensions.Options;
using HCS.Passwordless.Configuration;
using HCS.Passwordless.Notifications;
using HCS.Passwordless.Otp.Configuration;
using HCS.Passwordless.Otp.Notifications.Models;
using Umbraco.Cms.Core.Mail;
using Umbraco.Cms.Core.Models.Email;
using Umbraco.Cms.Core.Security;

namespace HCS.Passwordless.Otp.Notifications;

internal sealed class EmailOtpNotificationSender : IOtpNotificationSender
{
    private readonly IEmailSender _email;
    private readonly IRazorViewRenderer _renderer;
    private readonly IOptionsMonitor<OtpOptions> _otpOpts;
    private readonly IOptionsMonitor<PasswordlessOptions> _baseOpts;

    public EmailOtpNotificationSender(
        IEmailSender email,
        IRazorViewRenderer renderer,
        IOptionsMonitor<OtpOptions> otpOpts,
        IOptionsMonitor<PasswordlessOptions> baseOpts)
    {
        _email = email;
        _renderer = renderer;
        _otpOpts = otpOpts;
        _baseOpts = baseOpts;
    }

    public async Task SendOtpAsync(
        MemberIdentityUser member, string code, TimeSpan validFor, CancellationToken ct = default)
    {
        var otpOpts = _otpOpts.CurrentValue;
        var notifications = _baseOpts.CurrentValue.Notifications;

        var model = new OtpEmailModel
        {
            MemberName = string.IsNullOrEmpty(member.Name) ? (member.Email ?? "Member") : member.Name,
            Code = code,
            Expiry = validFor,
            Branding = notifications.Branding
        };

        var html = await _renderer.RenderAsync(otpOpts.NotificationPartial, model, ct);

        var msg = new EmailMessage(
            notifications.FromAddress,
            new[] { member.Email! },
            null,
            null,
            null,
            otpOpts.NotificationSubject,
            html,
            true,
            null);

        await _email.SendAsync(msg, emailType: "Passwordless.Otp", enableNotification: false, null);
    }
}
