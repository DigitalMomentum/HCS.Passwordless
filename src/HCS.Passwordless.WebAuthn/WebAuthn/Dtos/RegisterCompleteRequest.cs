namespace HCS.Passwordless.WebAuthn.Dtos;

public sealed record RegisterCompleteRequest(string CeremonyId, AttestationDto Attestation);

public sealed record AttestationDto(
    string Id,
    string RawId,
    string? Type,
    AttestationResponseDto Response);

public sealed record AttestationResponseDto(
    string AttestationObject,
    string ClientDataJSON,
    string[]? Transports);
