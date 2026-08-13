namespace ABCDMall.Modules.Events.Application.DTOs.Events;

public sealed class EventListQueryDto
{
    public string? Keyword { get; set; }

    /// <summary>1 = MallEvent, 2 = BrandEvent. Null = lấy tất cả.</summary>
    public int? EventType { get; set; }

    /// <summary>"upcoming" | "ongoing" | "ended". Null = lấy tất cả.</summary>
    public string? Status { get; set; }

    /// <summary>Nếu true, chỉ lấy sự kiện đang HOT (cho Banner Slider).</summary>
    public bool? IsHot { get; set; }
}