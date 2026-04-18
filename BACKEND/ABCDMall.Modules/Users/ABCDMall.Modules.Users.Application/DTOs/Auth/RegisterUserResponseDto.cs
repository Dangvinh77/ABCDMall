namespace ABCDMall.Modules.Users.Application.DTOs.Auth;

public sealed class RegisterUserResponseDto
{
    public string Message { get; set; } = string.Empty;

    public bool EmailSent { get; set; }

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public string? ShopId { get; set; }

    public string? CCCD { get; set; }

    public string ShopName { get; set; } = string.Empty;

    public DateTime? CreatedAt { get; set; }
}
