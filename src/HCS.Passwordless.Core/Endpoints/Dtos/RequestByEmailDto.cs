using System.ComponentModel.DataAnnotations;

namespace HCS.Passwordless.Endpoints.Dtos;

/// <summary>Request body for initiating a passwordless authentication flow by email address.</summary>
/// <param name="Email">The member's email address. Maximum 254 characters.</param>
/// <param name="ReturnUrl">Optional URL to redirect to after sign-in completes.</param>
public sealed record RequestByEmailDto([MaxLength(254)] string Email, string? ReturnUrl);
