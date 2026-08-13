using ABCDMall.Modules.Events.Domain.Enums;

namespace ABCDMall.Modules.Events.Domain.Entities;

public class Event
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string CoverImageUrl { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    /// <summary>
    /// Tên vị trí tổ chức. Ví dụ: "Sảnh Trung Tâm Tầng 1", "Khu FoodCourt Tầng 3".
    /// </summary>
    public string Location { get; set; } = string.Empty;

    public EventType EventType { get; set; } = EventType.MallEvent;

    /// <summary>
    /// Nullable. Chỉ set khi EventType = BrandEvent.
    /// Lưu Id của shop/nhãn hàng đứng ra tổ chức.
    /// Kiểu dữ liệu đồng bộ với module Shops: string.
    /// </summary>
    public string? ShopId { get; set; }

    /// <summary>
    /// Tên nhãn hàng/shop denormalized — tránh join cross-module.
    /// </summary>
    public string? ShopName { get; set; }

    /// <summary>
    /// Đánh dấu sự kiện HOT để hiển thị trên Banner Slider trang chủ.
    /// </summary>
    public bool IsHot { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
