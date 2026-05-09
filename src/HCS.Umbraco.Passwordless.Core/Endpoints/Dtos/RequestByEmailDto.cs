using System.ComponentModel.DataAnnotations;

namespace HCS.Umbraco.Passwordless.Endpoints.Dtos;

public sealed record RequestByEmailDto([MaxLength(254)] string Email, string? ReturnUrl);
