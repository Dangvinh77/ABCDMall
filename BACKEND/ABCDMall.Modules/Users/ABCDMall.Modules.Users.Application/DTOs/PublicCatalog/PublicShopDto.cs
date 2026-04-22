namespace ABCDMall.Modules.Users.Application.DTOs.PublicCatalog;

public sealed class PublicShopDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string? Badge { get; set; }
    public string? Offer { get; set; }
    public string OpenHours { get; set; } = "09:00 - 22:00";
    public string[] Tags { get; set; } = [];
    public string LogoUrl { get; set; } = string.Empty;
    public string CoverImageUrl { get; set; } = string.Empty;
    public string Floor { get; set; } = string.Empty;
    public string LocationSlot { get; set; } = string.Empty;
    public string ShopStatus { get; set; } = "Active";
    public DateTime? OpeningDate { get; set; }
    public IReadOnlyList<PublicShopProductDto> Products { get; set; } = [];
    public IReadOnlyList<PublicShopVoucherDto> Vouchers { get; set; } = [];
}

public sealed class PublicShopProductDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? OldPrice { get; set; }
    public int? DiscountPercent { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsDiscounted { get; set; }
}

public sealed class PublicShopVoucherDto
{
    public string Id { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ValidUntil { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
