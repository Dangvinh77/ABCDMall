namespace ABCDMall.Modules.Users.Application.DTOs.Auth;

public sealed class ProfileUpdateRequestResponseDto
{
    public string? Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? CurrentFullName { get; set; }
    public string? CurrentAddress { get; set; }
    public string? CurrentCCCD { get; set; }
    public string? CurrentCccdFrontImage { get; set; }
    public string? CurrentCccdBackImage { get; set; }
    public string? RequestedFullName { get; set; }
    public string? RequestedAddress { get; set; }
    public string? RequestedCCCD { get; set; }
    public string? RequestedCccdFrontImage { get; set; }
    public string? RequestedCccdBackImage { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ReviewedByAdminId { get; set; }
    public string? ReviewNote { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
}
