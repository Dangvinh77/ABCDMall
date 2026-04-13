// FRONTEND/src/features/shops/types/shop.types.ts

export interface Product {
  id: string;
  name: string;
  price: number;
  imageUrl: string;
}

export interface Voucher {
  id: string;
  code: string;
  title: string;
  description: string;
  validUntil: string;
}

export interface ShopDetail {
  id: string;
  slug: string; // ví dụ: "uniqlo"
  name: string;
  slogan: string;
  locationSlot: string;
  floor: string;
  openTime: string;
  closeTime: string;
  coverImageUrl: string;
  logoUrl: string;
  description: string;
  vouchers: Voucher[];
  products: Product[];
}