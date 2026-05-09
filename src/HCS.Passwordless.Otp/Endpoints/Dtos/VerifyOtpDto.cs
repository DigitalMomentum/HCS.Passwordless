using System.ComponentModel.DataAnnotations;

namespace HCS.Passwordless.Otp.Endpoints.Dtos;

public sealed record VerifyOtpDto([MaxLength(254)] string? Email, string? Code, string? ReturnUrl);
