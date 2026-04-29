namespace ABCDMall.Modules.Users.Application.DTOs.Auth;

public sealed class SeedForgotPasswordOtpResponseDto
{
    public string Email { get; set; } = string.Empty;

    public string Otp { get; set; } = string.Empty;

    public DateTime? ExpiresAt { get; set; }
}
