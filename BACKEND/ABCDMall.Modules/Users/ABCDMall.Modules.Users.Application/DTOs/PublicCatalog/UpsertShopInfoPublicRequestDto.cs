using Microsoft.AspNetCore.Http;

namespace ABCDMall.Modules.Users.Application.DTOs.PublicCatalog;

public sealed class UpsertShopInfoPublicRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Floor { get; set; } = string.Empty;
    public string LocationSlot { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? CoverImageUrl { get; set; }
    public IFormFile? LogoImage { get; set; }
    public IFormFile? CoverImage { get; set; }
    public string? ProductsJson { get; set; } = "[]";
    public List<IFormFile> ProductImages { get; set; } = [];
    public string OpenHours { get; set; } = "09:30 - 22:00";
    public string? Badge { get; set; }
    public string? Offer { get; set; }
    public string[] Tags { get; set; } = [];
    public DateTime? OpeningDate { get; set; }
}

public sealed class UpsertShopProductRequestDto
{
    public string? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int? ImageFileIndex { get; set; }
    public decimal Price { get; set; }
    public decimal? OldPrice { get; set; }
    public int? DiscountPercent { get; set; }
    public bool IsFeatured { get; set; } = true;
    public bool IsDiscounted { get; set; }
}
