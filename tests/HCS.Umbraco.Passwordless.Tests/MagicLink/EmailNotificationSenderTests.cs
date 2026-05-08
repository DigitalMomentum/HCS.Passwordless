using HCS.Umbraco.Passwordless.Configuration;
using HCS.Umbraco.Passwordless.MagicLink.Notifications;
using HCS.Umbraco.Passwordless.MagicLink.Notifications.Models;
using HCS.Umbraco.Passwordless.Notifications;
using Umbraco.Cms.Core.Mail;
using Umbraco.Cms.Core.Models.Email;
using Umbraco.Cms.Core.Security;

namespace HCS.Umbraco.Passwordless.Tests.MagicLink;

public class EmailNotificationSenderTests
{
    private readonly IEmailSender _emailSender = Substitute.For<IEmailSender>();
    private readonly IRazorViewRenderer _renderer = Substitute.For<IRazorViewRenderer>();

    private EmailNotificationSender CreateSut(NotificationOptions? notifications = null)
    {
        var opts = new PasswordlessOptions
        {
            Notifications = notifications ?? new NotificationOptions
            {
                FromAddress = "noreply@example.com",
                MagicLinkSubject = "Your sign-in link",
                MagicLinkPartial = "Emails/Passwordless/MagicLink",
                Branding = new BrandingOptions { ProductName = "Test Site" }
            }
        };
        var monitor = Substitute.For<IOptionsMonitor<PasswordlessOptions>>();
        monitor.CurrentValue.Returns(opts);
        return new EmailNotificationSender(_emailSender, _renderer, monitor);
    }

    [Fact]
    public async Task SendMagicLinkAsync_RendersConfiguredPartialView()
    {
        _renderer.RenderAsync(Arg.Any<string>(), Arg.Any<MagicLinkEmailModel>(), Arg.Any<CancellationToken>())
            .Returns("<html>link</html>");

        var member = new MemberIdentityUser { Email = "user@example.com", UserName = "user@example.com" };
        var sut = CreateSut();

        await sut.SendMagicLinkAsync(member, new Uri("https://example.com/verify?token=abc"), TimeSpan.FromMinutes(15));

        await _renderer.Received(1).RenderAsync(
            "Emails/Passwordless/MagicLink",
            Arg.Any<MagicLinkEmailModel>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendMagicLinkAsync_SendsEmailViaSender()
    {
        _renderer.RenderAsync(Arg.Any<string>(), Arg.Any<MagicLinkEmailModel>(), Arg.Any<CancellationToken>())
            .Returns("<html>link</html>");

        var member = new MemberIdentityUser { Email = "user@example.com", UserName = "user@example.com" };
        var sut = CreateSut();

        await sut.SendMagicLinkAsync(member, new Uri("https://example.com/verify"), TimeSpan.FromMinutes(15));

        await _emailSender.Received(1).SendAsync(
            Arg.Any<EmailMessage>(),
            "Passwordless.MagicLink",
            false);
    }

    [Fact]
    public async Task SendMagicLinkAsync_PassesMemberNameToModel()
    {
        MagicLinkEmailModel? capturedModel = null;
        _renderer.RenderAsync(Arg.Any<string>(), Arg.Do<MagicLinkEmailModel>(m => capturedModel = m), Arg.Any<CancellationToken>())
            .Returns("<html/>");

        var member = new MemberIdentityUser { UserName = "Jane", Email = "jane@example.com" };
        var sut = CreateSut();

        await sut.SendMagicLinkAsync(member, new Uri("https://example.com/v"), TimeSpan.FromMinutes(10));

        capturedModel.Should().NotBeNull();
        capturedModel!.Link.Should().Be(new Uri("https://example.com/v"));
        capturedModel.Expiry.Should().Be(TimeSpan.FromMinutes(10));
    }

    [Fact]
    public async Task SendMagicLinkAsync_FallsBackToEmailWhenNameIsNull()
    {
        MagicLinkEmailModel? capturedModel = null;
        _renderer.RenderAsync(Arg.Any<string>(), Arg.Do<MagicLinkEmailModel>(m => capturedModel = m), Arg.Any<CancellationToken>())
            .Returns("<html/>");

        var member = new MemberIdentityUser { Email = "noname@example.com", UserName = "noname@example.com" };
        var sut = CreateSut();

        await sut.SendMagicLinkAsync(member, new Uri("https://example.com/v"), TimeSpan.FromMinutes(15));

        capturedModel!.MemberName.Should().Be("noname@example.com");
    }
}
