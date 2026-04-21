namespace ABCDMall.Modules.Users.Domain.Entities;

public sealed class PublicShop
{
    public string Id { get; set; } = string.Empty;
    public string? OwnerShopId { get; set; }
    public string Name { get; set; } = string.Empty;
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
}
