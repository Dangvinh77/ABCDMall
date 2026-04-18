namespace ABCDMall.Modules.UtilityMap.Domain.Entities;

public class MapLocation
{
    public int Id { get; set; }

    public int FloorPlanId { get; set; }
    public FloorPlan FloorPlan { get; set; } = null!;

    public string ShopName { get; set; } = string.Empty;
    public string LocationSlot { get; set; } = string.Empty;
    public string ShopUrl { get; set; } = string.Empty;

    public double X { get; set; }
    public double Y { get; set; }

    public string StorefrontImageUrl { get; set; } = string.Empty;
}
