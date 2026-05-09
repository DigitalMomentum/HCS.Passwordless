using Umbraco.Cms.Core.Security;

namespace HCS.Passwordless.Otp.Notifications;

public interface IOtpNotificationSender
{
    Task SendOtpAsync(
        MemberIdentityUser member, string code, TimeSpan validFor, CancellationToken ct = default);
}
