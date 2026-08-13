namespace ABCDMall.Modules.Events.Application.DTOs;

public class EventDto
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string CoverImageUrl { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string Location { get; set; } = string.Empty;

    /// <summary>Tên loại sự kiện: "MallEvent" | "BrandEvent"</summary>
    public string EventType { get; set; } = string.Empty;

    public int EventTypeId { get; set; }

    public string? ShopId { get; set; }

    public string? ShopName { get; set; }

    public bool IsHot { get; set; }

    /// <summary>Computed: "Upcoming" | "Ongoing" | "Ended"</summary>
    public string Status { get; set; } = string.Empty;

    public int StatusId { get; set; }

    public DateTime CreatedAt { get; set; }
}
