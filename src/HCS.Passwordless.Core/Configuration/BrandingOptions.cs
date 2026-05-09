namespace HCS.Passwordless.Configuration;

/// <summary>Branding values used by the default email templates.</summary>
public sealed class BrandingOptions
{
    /// <summary>Product or site name shown in email headers.</summary>
    public string ProductName { get; set; } = "Your Site";

    /// <summary>Optional URL to a logo image included in HTML emails.</summary>
    public string? LogoUrl { get; set; }

    /// <summary>Hex colour used for call-to-action buttons. Defaults to <c>#2d6cdf</c>.</summary>
    public string AccentColor { get; set; } = "#2d6cdf";

    /// <summary>Optional HTML snippet appended to the email footer.</summary>
    public string? FooterHtml { get; set; }
}
