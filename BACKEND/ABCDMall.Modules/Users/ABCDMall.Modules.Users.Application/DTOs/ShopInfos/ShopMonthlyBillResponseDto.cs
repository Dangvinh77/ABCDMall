namespace ABCDMall.Modules.Users.Application.DTOs.ShopInfos;

public sealed class ShopMonthlyBillResponseDto
{
    public string? Id { get; set; }
    public string ShopName { get; set; } = string.Empty;
    public string? ManagerName { get; set; }
    public string? CCCD { get; set; }
    public string RentalLocation { get; set; } = string.Empty;
    public string Month { get; set; } = string.Empty;
    public string UsageMonth { get; set; } = string.Empty;
    public string BillingMonthKey { get; set; } = string.Empty;
    public string UsageMonthKey { get; set; } = string.Empty;
    public string LeaseStartDate { get; set; } = string.Empty;
    public string ElectricityUsage { get; set; } = string.Empty;
    public decimal ElectricityFee { get; set; }
    public string WaterUsage { get; set; } = string.Empty;
    public decimal WaterFee { get; set; }
    public decimal ServiceFee { get; set; }
    public int LeaseTermDays { get; set; }
    public decimal TotalDue { get; set; }
    public string PaymentStatus { get; set; } = "Unpaid";
    public string? PaidAtUtc { get; set; }
    public string? ContractImage { get; set; }
    public string? ContractImages { get; set; }
}
