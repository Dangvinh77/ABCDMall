namespace ABCDMall.Modules.UtilityMap.Domain.Entities;

public class MapLocation
{
    public int Id { get; set; }

    // FK về FloorPlan
    public int FloorPlanId { get; set; }
    public FloorPlan FloorPlan { get; set; } = null!;

    public string ShopName { get; set; } = string.Empty;
    public string LocationSlot { get; set; } = string.Empty;  // Mã lô: "L1-05"
    public string ShopUrl { get; set; } = string.Empty;       // Link tới trang chi tiết shop

    // Tọa độ % trên ảnh bản đồ (0.0 - 100.0)
    public double X { get; set; }
    public double Y { get; set; }

    public string StorefrontImageUrl { get; set; } = string.Empty;
}