namespace ABCDMall.Modules.Users.Application.DTOs.Bidding;

public sealed class AdminCarouselBidListItemDto
{
    public string Id { get; set; } = string.Empty;

    public string ShopId { get; set; } = string.Empty;

    public string ShopName { get; set; } = string.Empty;

    public string TemplateType { get; set; } = string.Empty;

    public decimal BidAmount { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime TargetMondayDate { get; set; }

    public DateTime CreatedAt { get; set; }
}
