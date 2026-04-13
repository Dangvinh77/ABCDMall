namespace ABCDMall.Shared.DTOs;

public class MapLocationDto
{
    public int Id { get; set; }
    public string ShopName { get; set; } = string.Empty;
    public string LocationSlot { get; set; } = string.Empty;
    public string ShopUrl { get; set; } = string.Empty;
    public double X { get; set; }
    public double Y { get; set; }
    public string StorefrontImageUrl { get; set; } = string.Empty;
}

public class FloorPlanDto
{
    public int Id { get; set; }
    public string FloorLevel { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string BlueprintImageUrl { get; set; } = string.Empty;
    public List<MapLocationDto> Locations { get; set; } = new();
}