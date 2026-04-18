namespace ABCDMall.Modules.Users.Application.DTOs
{
    public class UpdateMonthlyBillDto
    {
        public string BillingMonth { get; set; } = string.Empty;
        public string UsageMonth { get; set; } = string.Empty;
        public string ElectricityUsage { get; set; } = string.Empty;
        public string WaterUsage { get; set; } = string.Empty;
    }
}
