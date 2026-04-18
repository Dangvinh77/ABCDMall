namespace ABCDMall.Modules.Users.Application.DTOs.Auth;

public sealed class UserSummaryResponseDto
{
    public string? Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? ShopId { get; set; }
    public string? ShopName { get; set; }
    public string? Image { get; set; }
    public string? Address { get; set; }
    public string? CCCD { get; set; }
    public int FailedLoginAttempts { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
