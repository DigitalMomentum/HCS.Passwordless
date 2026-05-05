using HCS.Umbraco.Passwordless.Configuration;
using HCS.Umbraco.Passwordless.Notifications;
using HCS.Umbraco.Passwordless.Otp.Configuration;
using HCS.Umbraco.Passwordless.Otp.Notifications;
using HCS.Umbraco.Passwordless.Otp.Notifications.Models;
using Umbraco.Cms.Core.Mail;
using Umbraco.Cms.Core.Models.Email;
using Umbraco.Cms.Core.Security;

namespace HCS.Umbraco.Passwordless.Tests.Otp;

public class EmailOtpNotificationSenderTests
{
    private readonly IEmailSender _emailSender = Substitute.For<IEmailSender>();
    private readonly IRazorViewRenderer _renderer = Substitute.For<IRazorViewRenderer>();

    private EmailOtpNotificationSender CreateSut(
        OtpOptions? otpOpts = null,
        NotificationOptions? notifications = null)
    {
        var otp = otpOpts ?? new OtpOptions
        {
            NotificationSubject = "Your sign-in code",
            NotificationPartial = "Emails/Passwordless/Otp"
        };
        var baseOpts = new PasswordlessOptions
        {
            Notifications = notifications ?? new NotificationOptions
            {
                FromAddress = "noreply@example.com",
                FromName = "Test Site",
                Branding = new BrandingOptions { ProductName = "Test Site" }
            }
        };

        var otpMonitor = Substitute.For<IOptionsMonitor<OtpOptions>>();
        otpMonitor.CurrentValue.Returns(otp);

        var baseMonitor = Substitute.For<IOptionsMonitor<PasswordlessOptions>>();
        baseMonitor.CurrentValue.Returns(baseOpts);

        return new EmailOtpNotificationSender(_emailSender, _renderer, otpMonitor, baseMonitor);
    }

    [Fact]
    public async Task SendOtpAsync_RendersConfiguredPartialView()
    {
        _renderer.RenderAsync(Arg.Any<string>(), Arg.Any<OtpEmailModel>(), Arg.Any<CancellationToken>())
            .Returns("<html>code</html>");

        var member = new MemberIdentityUser { Email = "user@example.com" };
        var sut = CreateSut();

        await sut.SendOtpAsync(member, "123456", TimeSpan.FromMinutes(5));

        await _renderer.Received(1).RenderAsync(
            "Emails/Passwordless/Otp",
            Arg.Any<OtpEmailModel>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendOtpAsync_SendsEmailViaSender()
    {
        _renderer.RenderAsync(Arg.Any<string>(), Arg.Any<OtpEmailModel>(), Arg.Any<CancellationToken>())
            .Returns("<html>code</html>");

        var member = new MemberIdentityUser { Email = "user@example.com" };
        await CreateSut().SendOtpAsync(member, "123456", TimeSpan.FromMinutes(5));

        await _emailSender.Received(1).SendAsync(
            Arg.Any<EmailMessage>(),
            "Passwordless.Otp",
            false);
    }

    [Fact]
    public async Task SendOtpAsync_PassesCodeAndExpiryToModel()
    {
        OtpEmailModel? capturedModel = null;
        _renderer.RenderAsync(Arg.Any<string>(), Arg.Do<OtpEmailModel>(m => capturedModel = m), Arg.Any<CancellationToken>())
            .Returns("<html/>");

        var member = new MemberIdentityUser { Email = "user@example.com" };
        await CreateSut().SendOtpAsync(member, "987654", TimeSpan.FromMinutes(3));

        capturedModel.Should().NotBeNull();
        capturedModel!.Code.Should().Be("987654");
        capturedModel.Expiry.Should().Be(TimeSpan.FromMinutes(3));
    }

    [Fact]
    public async Task SendOtpAsync_FallsBackToEmail_WhenNameIsNull()
    {
        OtpEmailModel? capturedModel = null;
        _renderer.RenderAsync(Arg.Any<string>(), Arg.Do<OtpEmailModel>(m => capturedModel = m), Arg.Any<CancellationToken>())
            .Returns("<html/>");

        var member = new MemberIdentityUser { Email = "noname@example.com" };
        await CreateSut().SendOtpAsync(member, "111111", TimeSpan.FromMinutes(5));

        capturedModel!.MemberName.Should().Be("noname@example.com");
    }
}
