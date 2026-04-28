import { api } from "../../../core/api/api";

export type Shop = {
  id: string;
  name: string;
  slug: string;
  category: string;
  location: string;
  summary: string;
  description: string;
  imageUrl: string;
  logoUrl?: string;
  badge?: string;
  offer?: string;
  openHours: string;
  tags: string[];
};

export type ShopProduct = {
  id: string;
  name: string;
  imageUrl: string;
  price: number;
  oldPrice?: number | null;
  discountPercent?: number | null;
  isFeatured: boolean;
  isDiscounted: boolean;
};

export type ShopVoucher = {
  id: string;
  code: string;
  title: string;
  description: string;
  validUntil: string;
  isActive: boolean;
};

export type ShopDetail = Shop & {
  logoUrl: string;
  coverImageUrl: string;
  floor: string;
  locationSlot: string;
  products: ShopProduct[];
  vouchers: ShopVoucher[];
};

export type ManagedShopRequest = FormData;

export type AvailableRentalLocation = {
  locationSlot: string;
  floor: string;
  areaName?: string | null;
};

export type ShopCreationStatus = {
  shopCount: number;
  rentedAreaCount: number;
  canCreate: boolean;
  message: string;
  availableRentalLocations?: AvailableRentalLocation[];
};

export async function getShops(): Promise<Shop[]> {
  return api.get<Shop[]>("/shops");
}

export async function getShopBySlug(slug: string): Promise<ShopDetail | null> {
  try {
    return await api.get<ShopDetail>(`/shops/slug/${slug}`);
  } catch (error) {
    if (error instanceof Error && error.message.toLowerCase().includes("404")) {
      return null;
    }

    throw error;
  }
}

export async function getMyManagedShops(): Promise<ShopDetail[]> {
  return api.get<ShopDetail[]>("/shops/manager");
}

export async function getMyShopCreationStatus(): Promise<ShopCreationStatus> {
  return api.get<ShopCreationStatus>("/shops/manager/creation-status");
}

export async function createMyManagedShop(request: ManagedShopRequest): Promise<ShopDetail> {
  return api.post<ShopDetail, ManagedShopRequest>("/shops/manager", request);
}

export async function updateMyManagedShop(id: string, request: ManagedShopRequest): Promise<ShopDetail> {
  return api.put<ShopDetail, ManagedShopRequest>(`/shops/manager/${id}`, request);
}

export async function deleteMyManagedShop(id: string): Promise<void> {
  return api.delete<void>(`/shops/manager/${id}`);
}
