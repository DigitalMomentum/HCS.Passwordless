namespace HCS.Umbraco.Passwordless.WebAuthn.Dtos;

public sealed record SignInCompleteRequest(string CeremonyId, AssertionDto Assertion);

public sealed record AssertionDto(
    string Id,
    string RawId,
    string? Type,
    AssertionResponseDto Response);

public sealed record AssertionResponseDto(
    string AuthenticatorData,
    string ClientDataJson,
    string Signature,
    string? UserHandle);
