namespace ABCDMall.Modules.Users.Application.DTOs.Auth;

public sealed class CompleteInitialPasswordChangeDto
{
    public string Token { get; set; } = string.Empty;

    public string NewPassword { get; set; } = string.Empty;
}
