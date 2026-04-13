

namespace ABCDMall.Modules.UtilityMap.Domain.Entities;

public class FloorPlan
{
    public int Id { get; set; }
    
    // Tên tầng (Ví dụ: "L1", "L2", "L3", "L4")
    public string FloorLevel { get; set; } = string.Empty; 
    
    // Mô tả tầng (Ví dụ: "Tầng 1 - Thời trang cao cấp & Mỹ phẩm")
    public string Description { get; set; } = string.Empty;
    
    // Đường dẫn ảnh bản đồ 2D (SVG hoặc PNG)
    public string BlueprintImageUrl { get; set; } = string.Empty; 
    
    // Danh sách các cửa hàng nằm trên tầng này và tọa độ của chúng
    public List<MapLocation> Locations { get; set; } = new();
}

// Class phụ để lưu tọa độ từng shop trên bản đồ
public class MapLocation
{
    public int Id { get; set; }
    public string ShopId { get; set; } = string.Empty; 
    public string ShopName { get; set; } = string.Empty;
    public string LocationSlot { get; set; } = string.Empty; // Mã lô: L1-05
    
    // Tọa độ X, Y tính theo phần trăm (%) để bản đồ luôn chuẩn dù màn hình to hay nhỏ
    public double X_Coordinate { get; set; } 
    public double Y_Coordinate { get; set; } 
    
    public string StorefrontImageUrl { get; set; } = string.Empty; // Ảnh mặt tiền cửa hàng
}