using System.ComponentModel.DataAnnotations;

namespace HCS.Passwordless.WebAuthn.Dtos;

public sealed record SignInOptionsRequest([MaxLength(254)] string? Email);
