using System.ComponentModel.DataAnnotations;

namespace HCS.Umbraco.Passwordless.WebAuthn.Dtos;

public sealed record SignInOptionsRequest([MaxLength(254)] string? Email);
