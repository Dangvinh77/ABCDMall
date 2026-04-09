export interface MapLocation {
  shopId: string;
  shopName: string;
  locationSlot: string; // Ví dụ: L1-05
  x_Coordinate: number; // Tọa độ X (%)
  y_Coordinate: number; // Tọa độ Y (%)
  storefrontImageUrl: string;
}

export interface FloorPlan {
  id: string;
  floorLevel: string; // Ví dụ: "L1", "L2"
  description: string;
  blueprintImageUrl: string; // Ảnh mặt bằng SVG/PNG
  locations: MapLocation[];
}