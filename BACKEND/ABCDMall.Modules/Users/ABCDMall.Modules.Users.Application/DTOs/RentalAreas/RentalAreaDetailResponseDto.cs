namespace ABCDMall.Modules.Users.Application.DTOs.RentalAreas;

public sealed class RentalAreaDetailResponseDto
{
    public string? Id { get; set; }
    public string AreaCode { get; set; } = string.Empty;
    public string Floor { get; set; } = string.Empty;
    public string AreaName { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public decimal MonthlyRent { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? TenantName { get; set; }
    public string? ShopInfoId { get; set; }
    public string? ManagerName { get; set; }
    public string? CCCD { get; set; }
    public string? ShopName { get; set; }
    public string? RentalLocation { get; set; }
    public string? LeaseStartDate { get; set; }
    public decimal ElectricityFee { get; set; }
    public decimal WaterFee { get; set; }
    public decimal ServiceFee { get; set; }
    public int LeaseTermDays { get; set; }
    public string? ContractImage { get; set; }
    public string? ContractImages { get; set; }
}
