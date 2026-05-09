using Umbraco.Cms.Core.Security;

namespace HCS.Passwordless.Notifications;

public interface IPasswordlessNotificationSender
{
    Task SendMagicLinkAsync(
        MemberIdentityUser member, Uri magicLink, TimeSpan validFor, CancellationToken ct = default);
}
