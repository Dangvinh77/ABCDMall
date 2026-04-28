namespace ABCDMall.Modules.UtilityMap.Application.DTOs.Maps;

public class MapLocationDto
{
    public int Id { get; set; }
    public string ShopName { get; set; } = string.Empty;
    public string LocationSlot { get; set; } = string.Empty;
    public string ShopUrl { get; set; } = string.Empty;
    public double X { get; set; }
    public double Y { get; set; }
    public string StorefrontImageUrl { get; set; } = string.Empty;
    public string Status { get; set; } = "Available";
}
