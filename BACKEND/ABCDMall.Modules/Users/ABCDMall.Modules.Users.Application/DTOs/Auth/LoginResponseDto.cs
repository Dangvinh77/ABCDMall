namespace ABCDMall.Modules.Users.Application.DTOs.Auth;

public sealed class LoginResponseDto
{
    public string? AccessToken { get; set; }

    public string? RefreshToken { get; set; }

    public bool RequiresOtp { get; set; }

    public bool RequiresPasswordChange { get; set; }

    public string? PasswordSetupToken { get; set; }

    public string? Message { get; set; }
}
