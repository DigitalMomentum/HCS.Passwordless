namespace HCS.Passwordless.Configuration;

/// <summary>Options for the outbound notification emails (from-address, subjects, templates).</summary>
public sealed class NotificationOptions
{
    /// <summary>The <c>From</c> address used when sending authentication emails.</summary>
    public string FromAddress { get; set; } = "no-reply@example.com";

    /// <summary>The display name used in the <c>From</c> header of authentication emails.</summary>
    public string FromName { get; set; } = "Example";

    /// <summary>Subject line for magic-link emails.</summary>
    public string MagicLinkSubject { get; set; } = "Your sign-in link";

    /// <summary>Razor partial view path for the HTML magic-link email body.</summary>
    public string MagicLinkPartial { get; set; } = "Emails/Passwordless/MagicLink";

    /// <summary>Razor partial view path for the plain-text magic-link email body.</summary>
    public string MagicLinkTextPartial { get; set; } = "Emails/Passwordless/MagicLink.Text";

    /// <summary>Visual branding values injected into the default email templates.</summary>
    public BrandingOptions Branding { get; set; } = new();
}
