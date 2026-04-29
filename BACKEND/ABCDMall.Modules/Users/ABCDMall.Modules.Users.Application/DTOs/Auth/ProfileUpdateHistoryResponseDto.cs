namespace ABCDMall.Modules.Users.Application.DTOs.Auth;

public sealed class ProfileUpdateHistoryResponseDto
{
    public string? Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? PreviousFullName { get; set; }
    public string? PreviousAddress { get; set; }
    public string? PreviousImage { get; set; }
    public string? PreviousCCCD { get; set; }
    public string? PreviousCccdFrontImage { get; set; }
    public string? PreviousCccdBackImage { get; set; }
    public string? UpdatedFullName { get; set; }
    public string? UpdatedAddress { get; set; }
    public string? UpdatedImage { get; set; }
    public string? UpdatedCCCD { get; set; }
    public string? UpdatedCccdFrontImage { get; set; }
    public string? UpdatedCccdBackImage { get; set; }
    public DateTime UpdatedAt { get; set; }
}
