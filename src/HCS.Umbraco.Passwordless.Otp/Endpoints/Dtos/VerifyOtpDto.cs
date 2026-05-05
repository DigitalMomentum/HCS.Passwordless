namespace HCS.Umbraco.Passwordless.Otp.Endpoints.Dtos;

public sealed record VerifyOtpDto(string? Email, string? Code, string? ReturnUrl);
