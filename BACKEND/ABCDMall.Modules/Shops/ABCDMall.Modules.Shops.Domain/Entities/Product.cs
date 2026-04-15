namespace ABCDMall.Modules.Shops.Domain.Entities;

public class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ShopId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? OldPrice { get; set; } // Null nếu không giảm giá
    public int? DiscountPercent { get; set; }
    public bool IsFeatured { get; set; } = false;
    public bool IsDiscounted { get; set; } = false;

    // Navigation Property
    public Shop Shop { get; set; } = null!;
}