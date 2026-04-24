namespace ABCDMall.Modules.Users.Application.DTOs.Bidding;

public sealed class PublicCarouselItemDto
{
    public string Id { get; set; } = string.Empty;

    public string SlotType { get; set; } = string.Empty;

    public DateTime TargetMondayDate { get; set; }

    public string? ShopId { get; set; }

    public string? ShopName { get; set; }

    public string? ShopSlug { get; set; }

    public string ImageUrl { get; set; } = string.Empty;

    public string? Message { get; set; }

    public string? Description { get; set; }

    public decimal? OriginalPrice { get; set; }

    public decimal? DiscountPrice { get; set; }

    public DateTime? EventStartDate { get; set; }

    public string? StartTime { get; set; }

    public string? LinkUrl { get; set; }
}
