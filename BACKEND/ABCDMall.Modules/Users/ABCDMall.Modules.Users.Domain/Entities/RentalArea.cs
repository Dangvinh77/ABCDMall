namespace ABCDMall.Modules.Users.Domain.Entities
{
    public class RentalArea
    {
        public string? Id { get; set; } = Guid.NewGuid().ToString("N");

        public string AreaCode { get; set; } = string.Empty;

        public string Floor { get; set; } = string.Empty;

        public string AreaName { get; set; } = string.Empty;

        public string Size { get; set; } = string.Empty;

        public decimal MonthlyRent { get; set; }

        public string Status { get; set; } = "Available";

        public string? TenantName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
