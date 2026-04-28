namespace ABCDMall.Modules.Users.Application.DTOs.Auth;

public sealed class DebugOtpLookupRequestDto
{
    public string? Email { get; set; }

    public string? UserId { get; set; }

    public bool RegenerateInitialPasswordOtp { get; set; }
}
