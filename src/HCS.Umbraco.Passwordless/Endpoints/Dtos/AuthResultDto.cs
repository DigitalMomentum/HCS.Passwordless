namespace HCS.Umbraco.Passwordless.Endpoints.Dtos;

public sealed record AuthResultDto(bool Success, string? RedirectTo, string? Error);
