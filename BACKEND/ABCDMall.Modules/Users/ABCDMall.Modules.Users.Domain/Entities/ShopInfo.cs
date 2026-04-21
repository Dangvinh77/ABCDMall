namespace ABCDMall.Modules.Users.Domain.Entities
{
    public class ShopInfo
    {
        public string? Id { get; set; } = Guid.NewGuid().ToString("N");

        public string? OwnerShopInfoId { get; set; }

        public string ShopName { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string Floor { get; set; } = string.Empty;

        public string LocationSlot { get; set; } = string.Empty;

        public string Summary { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string LogoUrl { get; set; } = string.Empty;

        public string CoverImageUrl { get; set; } = string.Empty;

        public string OpenHours { get; set; } = "09:30 - 22:00";

        public string? Badge { get; set; }

        public string? Offer { get; set; }

        public string Tags { get; set; } = string.Empty;

        public bool IsPublicVisible { get; set; }

        public string? ManagerName { get; set; }

        public string? CCCD { get; set; }

        public string RentalLocation { get; set; } = string.Empty;

        public string Month { get; set; } = string.Empty;

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

          public DateTime? OpeningDate { get; set; }
    }
}
