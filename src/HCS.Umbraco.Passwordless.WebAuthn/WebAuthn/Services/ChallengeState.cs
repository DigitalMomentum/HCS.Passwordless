using Fido2NetLib;

namespace HCS.Umbraco.Passwordless.WebAuthn.Services;

internal sealed record RegistrationCeremonyState(CredentialCreateOptions Options, string? Nickname, Guid MemberKey);

internal sealed record AssertionCeremonyState(AssertionOptions Options, Guid? MemberKey, bool IsDecoy);
