namespace ABCDMall.Modules.Shops.Application.DTOs;

public class ShopDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public string CoverImageUrl { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Slogan { get; set; } = string.Empty;
    public string Floor { get; set; } = string.Empty;
    public string LocationSlot { get; set; } = string.Empty;
    public string OpenTime { get; set; } = string.Empty;
    public string CloseTime { get; set; } = string.Empty;

    public List<ProductDto> Products { get; set; } = new();
    public List<VoucherDto> Vouchers { get; set; } = new();
}

public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? OldPrice { get; set; }
    public int? DiscountPercent { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsDiscounted { get; set; }
}

public class VoucherDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ValidUntil { get; set; } = string.Empty;
}