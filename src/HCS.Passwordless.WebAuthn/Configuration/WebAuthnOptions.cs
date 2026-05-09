using Fido2NetLib.Objects;

namespace HCS.Passwordless.WebAuthn.Configuration;

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
    /// <summary>
    /// Reserved for future FIDO Alliance Metadata Service (MDS) integration.
    /// This property is not currently implemented — setting it to <c>true</c> has no effect.
    /// </summary>
    public bool MetadataService { get; set; } = false;
    public bool RequireAtLeastOneNonPasskeyFactor { get; set; } = true;

    /// <summary>
    /// Secret key (≥ 16 bytes when UTF-8 encoded) used to generate decoy credential IDs for
    /// email-enumeration protection. Must be set to a random, operator-specific value — never
    /// leave this at the default or the decoys can be pre-computed from the NuGet binary.
    /// </summary>
    public string? DecoyHmacKey { get; set; }

    /// <summary>Maximum sign-in options requests per IP per minute (default 10).</summary>
    public int SignInOptionsPerIpPerMinute { get; set; } = 10;

    /// <summary>Maximum sign-in complete requests per IP per minute (default 5).</summary>
    public int SignInCompletePerIpPerMinute { get; set; } = 5;

    /// <summary>
    /// Allowed clock-skew window for WebAuthn timestamp validation, in milliseconds.
    /// Must be between 30 000 ms (30 s) and 600 000 ms (10 min). Default is 300 000 ms (5 min).
    /// </summary>
    public int TimestampDriftToleranceMs { get; set; } = 300_000;
}
