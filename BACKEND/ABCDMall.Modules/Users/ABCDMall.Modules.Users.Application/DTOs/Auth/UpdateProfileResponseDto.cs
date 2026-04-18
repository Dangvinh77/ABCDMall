namespace ABCDMall.Modules.Users.Application.DTOs.Auth;

public sealed class UpdateProfileResponseDto
{
    public string Message { get; set; } = string.Empty;

    public UserProfileResponseDto Profile { get; set; } = new();
}
