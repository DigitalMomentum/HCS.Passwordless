namespace HCS.Umbraco.Passwordless.Endpoints.Dtos;

public sealed record RequestByEmailDto(string Email, string? ReturnUrl);
