namespace ABCDMall.Modules.UtilityMap.Domain.Entities;

public class FloorPlan
{
    public int Id { get; set; }
    public string FloorLevel { get; set; } = string.Empty;      
    public string Description { get; set; } = string.Empty;
    public string BlueprintImageUrl { get; set; } = string.Empty;

    public List<MapLocation> Locations { get; set; } = new();
}
