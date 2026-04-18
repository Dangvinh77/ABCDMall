namespace ABCDMall.Modules.Users.Application.DTOs.RentalAreas;

public sealed class RentalAreaResponseDto
{
    public string? Id { get; set; }
    public string AreaCode { get; set; } = string.Empty;
    public string Floor { get; set; } = string.Empty;
    public string AreaName { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public decimal MonthlyRent { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? TenantName { get; set; }
}
