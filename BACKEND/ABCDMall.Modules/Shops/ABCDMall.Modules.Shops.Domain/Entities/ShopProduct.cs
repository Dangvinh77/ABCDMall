namespace ABCDMall.Modules.Shops.Domain.Entities;

public sealed class ShopProduct
{
    public string Id { get; set; } = string.Empty;
    public string ShopId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? OldPrice { get; set; }
    public int? DiscountPercent { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsDiscounted { get; set; }
    public Shop Shop { get; set; } = null!;
}
