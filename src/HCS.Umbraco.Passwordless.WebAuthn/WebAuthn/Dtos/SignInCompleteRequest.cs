using Fido2NetLib;

namespace HCS.Umbraco.Passwordless.WebAuthn.Dtos;

public sealed record SignInCompleteRequest(string CeremonyId, AuthenticatorAssertionRawResponse Assertion);
