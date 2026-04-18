namespace ABCDMall.Modules.Shops.Application.DTOs;

public class ShopCatalogItemDto
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
    public string OpenHours { get; set; } = string.Empty;
    public string[] Tags { get; set; } = [];
}
