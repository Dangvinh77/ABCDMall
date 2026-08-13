namespace ABCDMall.Modules.Events.Application.DTOs.Events;

public sealed class UpdateEventRequestDto
{
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? CoverImageUrl { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string Location { get; set; } = string.Empty;

    /// <summary>1 = MallEvent, 2 = BrandEvent</summary>
    public int EventType { get; set; } = 1;

    public string? ShopId { get; set; }

    public string? ShopName { get; set; }

    public bool IsHot { get; set; }
}
