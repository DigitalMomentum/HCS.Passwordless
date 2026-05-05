using Umbraco.Cms.Core.Security;

namespace HCS.Umbraco.Passwordless.Notifications;

public interface IPasswordlessNotificationSender
{
    Task SendMagicLinkAsync(
        MemberIdentityUser member, Uri magicLink, TimeSpan validFor, CancellationToken ct = default);
}
