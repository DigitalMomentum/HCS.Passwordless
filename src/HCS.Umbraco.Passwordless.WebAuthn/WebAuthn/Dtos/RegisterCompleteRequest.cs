using Fido2NetLib;

namespace HCS.Umbraco.Passwordless.WebAuthn.Dtos;

public sealed record RegisterCompleteRequest(string CeremonyId, AuthenticatorAttestationRawResponse Attestation);
