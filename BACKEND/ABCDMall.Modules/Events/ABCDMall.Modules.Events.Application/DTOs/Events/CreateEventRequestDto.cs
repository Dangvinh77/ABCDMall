namespace ABCDMall.Modules.Events.Application.DTOs.Events;

public sealed class CreateEventRequestDto
{
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? CoverImageUrl { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string Location { get; set; } = string.Empty;

    /// <summary>1 = MallEvent, 2 = BrandEvent</summary>
    public int EventType { get; set; } = 1;

    /// <summary>Bắt buộc khi EventType = 2 (BrandEvent).</summary>
    public string? ShopId { get; set; }

    /// <summary>Tên nhãn hàng/shop. Denormalized — không cần join khi hiển thị.</summary>
    public string? ShopName { get; set; }

    public bool IsHot { get; set; }
}
