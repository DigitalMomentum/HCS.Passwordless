using System.ComponentModel.DataAnnotations;

namespace HCS.Umbraco.Passwordless.Otp.Endpoints.Dtos;

public sealed record VerifyOtpDto([MaxLength(254)] string? Email, string? Code, string? ReturnUrl);
