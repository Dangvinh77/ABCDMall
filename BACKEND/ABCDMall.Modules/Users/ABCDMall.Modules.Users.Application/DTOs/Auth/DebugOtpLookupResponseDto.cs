namespace ABCDMall.Modules.Users.Application.DTOs.Auth;

public sealed class DebugOtpLookupResponseDto
{
    public string UserId { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public string? LoginOtp { get; set; }

    public DateTime? LoginOtpExpiresAt { get; set; }

    public string? ForgotPasswordOtp { get; set; }

    public DateTime? ForgotPasswordOtpExpiresAt { get; set; }

    public string? ResetPasswordOtp { get; set; }

    public DateTime? ResetPasswordOtpExpiresAt { get; set; }

    public bool MustChangePassword { get; set; }

    public string? PasswordSetupToken { get; set; }

    public string? ChangePasswordUrl { get; set; }

    public DateTime? InitialPasswordExpiresAt { get; set; }

    public string? InitialPasswordOtp { get; set; }

    public bool InitialPasswordOtpReissued { get; set; }
}
