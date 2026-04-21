namespace ABCDMall.Modules.UtilityMap.Application.DTOs.Maps;

/// <summary>
/// DTO dành riêng cho Admin map view — bao gồm đầy đủ thông tin slot
/// bao gồm cả các slot "Available" mà public không thấy.
/// </summary>
public sealed class MapLocationAdminDto
{
    public int Id { get; set; }
    public string ShopName { get; set; } = string.Empty;
    public string LocationSlot { get; set; } = string.Empty;
    public string ShopUrl { get; set; } = string.Empty;
    public double X { get; set; }
    public double Y { get; set; }
    public string StorefrontImageUrl { get; set; } = string.Empty;

    // Trạng thái slot: "Available" | "Reserved" | "ComingSoon" | "Active"
    public string Status { get; set; } = "Available";

    // ShopInfo đang chiếm slot này (null nếu Available)
    public string? ShopInfoId { get; set; }
}