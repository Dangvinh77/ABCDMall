export interface Product {
  id: string;
  name: string;
  price: number;
  oldPrice?: number;
  discountPercent?: number; 
  imageUrl: string;
  isFeatured?: boolean; 
  isDiscounted?: boolean; 
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
  slug: string;
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
