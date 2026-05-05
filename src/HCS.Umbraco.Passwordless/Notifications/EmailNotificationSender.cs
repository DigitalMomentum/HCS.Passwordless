using Microsoft.Extensions.Options;
using HCS.Umbraco.Passwordless.Configuration;
using HCS.Umbraco.Passwordless.Notifications.Models;
using Umbraco.Cms.Core.Mail;
using Umbraco.Cms.Core.Models.Email;
using Umbraco.Cms.Core.Security;

namespace HCS.Umbraco.Passwordless.Notifications;

internal sealed class EmailNotificationSender : IPasswordlessNotificationSender
{
    private readonly IEmailSender _email;
    private readonly IRazorViewRenderer _renderer;
    private readonly IOptionsMonitor<PasswordlessOptions> _opts;

    public EmailNotificationSender(
        IEmailSender email,
        IRazorViewRenderer renderer,
        IOptionsMonitor<PasswordlessOptions> opts)
    {
        _email = email;
        _renderer = renderer;
        _opts = opts;
    }

    public async Task SendMagicLinkAsync(
        MemberIdentityUser member, Uri magicLink, TimeSpan validFor, CancellationToken ct = default)
    {
        var opts = _opts.CurrentValue.Notifications;
        var model = new MagicLinkEmailModel
        {
            MemberName = string.IsNullOrWhiteSpace(member.Name) ? (member.Email ?? "Member") : member.Name,
            Link = magicLink,
            Expiry = validFor,
            Branding = opts.Branding
        };

        var html = await _renderer.RenderAsync(opts.MagicLinkPartial, model, ct);

        var msg = new EmailMessage(
            opts.FromAddress,
            [member.Email!],
            null,
            null,
            null,
            opts.MagicLinkSubject,
            html,
            true,
            null);

        await _email.SendAsync(msg, emailType: "Passwordless.MagicLink", enableNotification: false);
    }
}
