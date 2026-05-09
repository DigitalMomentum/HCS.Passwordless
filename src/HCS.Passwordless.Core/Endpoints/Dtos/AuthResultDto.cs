namespace HCS.Passwordless.Endpoints.Dtos;

/// <summary>Response body returned by passwordless authentication endpoints.</summary>
/// <param name="Success">Whether the authentication attempt succeeded.</param>
/// <param name="RedirectTo">URL to redirect to when <paramref name="Success"/> is <c>true</c>.</param>
/// <param name="Error">Human-readable error message when <paramref name="Success"/> is <c>false</c>.</param>
public sealed record AuthResultDto(bool Success, string? RedirectTo, string? Error);
