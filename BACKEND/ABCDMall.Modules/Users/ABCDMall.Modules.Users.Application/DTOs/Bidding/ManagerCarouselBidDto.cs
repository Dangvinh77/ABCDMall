namespace ABCDMall.Modules.Users.Application.DTOs.Bidding;

public sealed class ManagerCarouselBidDto
{
    public string Id { get; set; } = string.Empty;

    public string ShopId { get; set; } = string.Empty;

    public string? ShopName { get; set; }

    public string TemplateType { get; set; } = string.Empty;

    public decimal BidAmount { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime TargetMondayDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? ImageUrl { get; set; }

    public string? Message { get; set; }

    public decimal? OriginalPrice { get; set; }

    public decimal? DiscountPrice { get; set; }

    public DateTime? EventStartDate { get; set; }

    public string? StartTime { get; set; }
}
