namespace HCS.Umbraco.Passwordless.Configuration;

public sealed class BrandingOptions
{
    public string ProductName { get; set; } = "Your Site";
    public string? LogoUrl { get; set; }
    public string AccentColor { get; set; } = "#2d6cdf";
    public string? FooterHtml { get; set; }
}
