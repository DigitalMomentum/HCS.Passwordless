using Fido2NetLib.Objects;

namespace HCS.Umbraco.Passwordless.WebAuthn.Configuration;

public sealed class WebAuthnOptions
{
    public const string SectionName = "HCS:Authentication:WebAuthn";

    public bool Enabled { get; set; } = true;
    public string? RpId { get; set; }
    public string RpName { get; set; } = "Umbraco Site";
    public HashSet<string> Origins { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public uint TimeoutMs { get; set; } = 60_000;
    public UserVerificationRequirement UserVerification { get; set; } = UserVerificationRequirement.Required;
    public AttestationConveyancePreference AttestationPreference { get; set; } = AttestationConveyancePreference.None;
    public ResidentKeyRequirement ResidentKey { get; set; } = ResidentKeyRequirement.Required;
    public AuthenticatorAttachment? AuthenticatorAttachment { get; set; }
    public TimeSpan ChallengeTtl { get; set; } = TimeSpan.FromMinutes(5);
    public bool MetadataService { get; set; } = false;
    public bool RequireAtLeastOneNonPasskeyFactor { get; set; } = true;
}
