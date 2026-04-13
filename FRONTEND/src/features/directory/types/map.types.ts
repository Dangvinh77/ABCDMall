export interface MapLocation {
  id: number;
  shopName: string;
  locationSlot: string;
  shopUrl: string;
  x: number;
  y: number;
  storefrontImageUrl: string;
}

export interface FloorPlan {
  id: number;
  floorLevel: string;
  description: string;
  blueprintImageUrl: string;
  locations: MapLocation[];
}