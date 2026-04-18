namespace ABCDMall.Modules.Users.Domain.Entities
{
    public class ShopMonthlyBill
    {
        public string? Id { get; set; } = Guid.NewGuid().ToString("N");

        public string ShopInfoId { get; set; } = string.Empty;

        public string BillKey { get; set; } = string.Empty;

        public string ShopName { get; set; } = string.Empty;

        public string? ManagerName { get; set; }

        public string? CCCD { get; set; }

        public string RentalLocation { get; set; } = string.Empty;

        public string Month { get; set; } = string.Empty;

        public string UsageMonth { get; set; } = string.Empty;

        public string BillingMonthKey { get; set; } = string.Empty;

        public string UsageMonthKey { get; set; } = string.Empty;

        public DateTime LeaseStartDate { get; set; }

        public string ElectricityUsage { get; set; } = string.Empty;

        public decimal ElectricityFee { get; set; }

        public string WaterUsage { get; set; } = string.Empty;

        public decimal WaterFee { get; set; }

        public decimal ServiceFee { get; set; }

        public int LeaseTermDays { get; set; }

        public decimal TotalDue { get; set; }

        public string? ContractImage { get; set; }

        public string? ContractImages { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
