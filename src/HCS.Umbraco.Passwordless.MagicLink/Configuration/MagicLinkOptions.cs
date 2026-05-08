namespace HCS.Umbraco.Passwordless.Configuration;

public sealed class MagicLinkOptions
{
    public const string SectionName = "HCS:Authentication:MagicLink";

    public bool Enabled { get; set; } = true;
    public TimeSpan TokenLifespan { get; set; } = TimeSpan.FromMinutes(15);
    public bool SingleUse { get; set; } = true;
}
