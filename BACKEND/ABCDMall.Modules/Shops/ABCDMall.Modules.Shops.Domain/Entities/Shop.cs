namespace ABCDMall.Modules.Shops.Domain.Entities;

public class Shop
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public string CoverImageUrl { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Slogan { get; set; } = string.Empty;
    public string Floor { get; set; } = string.Empty;
    public string LocationSlot { get; set; } = string.Empty;
    public string OpenTime { get; set; } = "09:30";
    public string CloseTime { get; set; } = "22:00";
    
    // Navigation Properties
    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<Voucher> Vouchers { get; set; } = new List<Voucher>();
}