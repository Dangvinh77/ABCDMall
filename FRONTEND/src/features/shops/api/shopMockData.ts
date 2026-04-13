// FRONTEND/src/features/shops/api/shopMockData.ts
import { ShopDetail } from '../types/shop.types';

export const mockShopData: Record<string, ShopDetail> = {
  "uniqlo": {
    id: "shop_1",
    slug: "uniqlo",
    name: "UNIQLO",
    slogan: "LifeWear - Thời trang mang lại cuộc sống tốt đẹp hơn",
    locationSlot: "1-01",
    floor: "Tầng 1",
    openTime: "09:30",
    closeTime: "22:00",
    coverImageUrl: "https://images.unsplash.com/photo-1441986300917-64674bd600d8?q=80&w=1920&auto=format&fit=crop", // Ảnh cover góc khác
    logoUrl: "https://upload.wikimedia.org/wikipedia/commons/thumb/9/92/UNIQLO_logo.svg/1024px-UNIQLO_logo.svg.png",
    description: "Cửa hàng Uniqlo tại ABCD Mall mang đến trải nghiệm mua sắm không gian mở, cung cấp đầy đủ các dòng sản phẩm LifeWear dành cho nam, nữ và trẻ em.",
    vouchers: [
      {
        id: "v1",
        code: "UNIQLO2026",
        title: "Giảm 100K",
        description: "Cho hóa đơn từ 1.000.000đ. Áp dụng tại cửa hàng.",
        validUntil: "30/04/2026"
      },
      {
        id: "v2",
        code: "NEWAPP50",
        title: "Free Heattech",
        description: "Tặng 1 áo Heattech cho KH tải app lần đầu.",
        validUntil: "15/05/2026"
      }
    ],
    products: [
      {
        id: "p1",
        name: "Áo Thun DRY-EX Cổ Tròn Ngắn Tay",
        price: 299000,
        imageUrl: "https://images.unsplash.com/photo-1521572163474-6864f9cf17ab?q=80&w=500&auto=format&fit=crop"
      },
      {
        id: "p2",
        name: "Áo Khoác AIRism Chống UV",
        price: 499000,
        imageUrl: "https://images.unsplash.com/photo-1556821840-3a63f95609a7?q=80&w=500&auto=format&fit=crop"
      },
      {
        id: "p3",
        name: "Quần Smart Ankle Pants",
        price: 799000,
        imageUrl: "https://images.unsplash.com/photo-1594938298596-70f56fb3cecb?q=80&w=500&auto=format&fit=crop"
      },
      {
        id: "p4",
        name: "Áo Sơ Mi Vải Linen Cao Cấp",
        price: 599000,
        imageUrl: "https://images.unsplash.com/photo-1598032895397-b9472444bf93?q=80&w=500&auto=format&fit=crop"
      }
    ]
  }
};