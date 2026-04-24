using ABCDMall.Modules.Users.Domain.Enums;

namespace ABCDMall.Modules.Users.Domain.Entities;

public class CarouselBid
{
    public string? Id { get; set; } = Guid.NewGuid().ToString("N");

    public string ShopId { get; set; } = string.Empty;

    public decimal BidAmount { get; set; }

    public CarouselBidTemplateType TemplateType { get; set; }

    public string TemplateData { get; set; } = string.Empty;

    public CarouselBidStatus Status { get; set; } = CarouselBidStatus.Pending;

    public DateTime TargetMondayDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
