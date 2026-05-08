namespace HCS.Umbraco.Passwordless.Configuration;

public sealed class NotificationOptions
{
    public string FromAddress { get; set; } = "no-reply@example.com";
    public string FromName { get; set; } = "Example";
    public string MagicLinkSubject { get; set; } = "Your sign-in link";
    public string MagicLinkPartial { get; set; } = "Emails/Passwordless/MagicLink";
    public string MagicLinkTextPartial { get; set; } = "Emails/Passwordless/MagicLink.Text";
    public BrandingOptions Branding { get; set; } = new();
}
