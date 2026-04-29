namespace ABCDMall.Modules.Users.Application.DTOs.ShopInfos;

public sealed class ShopRentalInfoResponseDto
{
    public string? ShopInfoId { get; set; }
    public string ShopName { get; set; } = string.Empty;
    public string? ManagerName { get; set; }
    public string? CCCD { get; set; }
    public string RentalLocation { get; set; } = string.Empty;
    public string Floor { get; set; } = string.Empty;
    public string LeaseStartDate { get; set; } = string.Empty;
    public decimal ElectricityFee { get; set; }
    public decimal WaterFee { get; set; }
    public decimal ServiceFee { get; set; }
    public int LeaseTermDays { get; set; }
    public string? ContractImage { get; set; }
    public string? ContractImages { get; set; }
}
