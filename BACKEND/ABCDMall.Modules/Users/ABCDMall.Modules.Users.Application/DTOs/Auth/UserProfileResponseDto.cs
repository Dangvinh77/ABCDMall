namespace ABCDMall.Modules.Users.Application.DTOs.Auth;

public sealed class UserProfileResponseDto
{
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? ShopId { get; set; }
    public string? Image { get; set; }
    public string? Address { get; set; }
    public string? CCCD { get; set; }
    public string? CccdFrontImage { get; set; }
    public string? CccdBackImage { get; set; }
}
