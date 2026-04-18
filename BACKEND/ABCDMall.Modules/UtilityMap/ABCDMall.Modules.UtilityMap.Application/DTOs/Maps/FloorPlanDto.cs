namespace ABCDMall.Modules.UtilityMap.Application.DTOs.Maps;

public class FloorPlanDto
{
    public int Id { get; set; }
    public string FloorLevel { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string BlueprintImageUrl { get; set; } = string.Empty;
    public List<MapLocationDto> Locations { get; set; } = new();
}
