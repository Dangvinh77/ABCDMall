import { useParams, Link } from 'react-router-dom';
import { useState, useEffect } from 'react';
import { mockShopData } from './api/shopMockData';
import { ShopDetail } from './types/shop.types';

export const ShopDetailFeature = () => {
  const { slug } = useParams<{ slug: string }>();
  const [shop, setShop] = useState<ShopDetail | null>(null);

  useEffect(() => {
    if (slug && mockShopData[slug.toLowerCase()]) {
      setShop(mockShopData[slug.toLowerCase()]);
    }
  }, [slug]);

  if (!shop) return <div className="p-20 text-center">Đang tải thông tin gian hàng...</div>;

  return (
    <div className="bg-white min-h-screen pb-20">
      {/* 1. HERO BANNER - Ảnh góc khác của cửa hiệu [cite: 9] */}
      <div className="relative h-[450px] w-full overflow-hidden">
        <img src={shop.coverImageUrl} className="w-full h-full object-cover" alt="Cover" />
        <div className="absolute inset-0 bg-black/40" />
        <div className="absolute bottom-10 left-10 md:left-20 flex items-end gap-6">
          <div className="w-32 h-32 bg-white rounded-2xl p-2 shadow-xl border-4 border-white">
            <img src={shop.logoUrl} className="w-full h-full object-contain" alt="Logo" />
          </div>
          <div className="text-white pb-2">
            <h1 className="text-5xl font-black uppercase tracking-tight">{shop.name}</h1>
            <p className="text-xl font-medium opacity-90 italic">"{shop.slogan}"</p>
          </div>
        </div>
      </div>

      {/* 2. THÔNG TIN CHI TIẾT [cite: 14] */}
      <div className="max-w-7xl mx-auto px-10 py-16 grid grid-cols-1 lg:grid-cols-3 gap-12">
        <div className="lg:col-span-2">
          <h2 className="text-2xl font-bold mb-4">Giới thiệu gian hàng</h2>
          <p className="text-gray-600 leading-relaxed text-lg mb-8">{shop.description}</p>
          
          {/* 3. DANH SÁCH SẢN PHẨM [cite: 14] */}
          <h2 className="text-2xl font-bold mb-6">Sản phẩm nổi bật</h2>
          <div className="grid grid-cols-2 md:grid-cols-3 gap-6">
            {shop.products.map(p => (
              <div key={p.id} className="group border rounded-2xl p-4 hover:shadow-lg transition">
                <img src={p.imageUrl} className="w-full aspect-square object-cover rounded-xl mb-4" />
                <h3 className="font-bold group-hover:text-red-500 transition">{p.name}</h3>
                <p className="text-red-600 font-black">{p.price.toLocaleString()} VNĐ</p>
              </div>
            ))}
          </div>
        </div>

        {/* 4. SIDEBAR: GIỜ MỞ CỬA & VOUCHER [cite: 14, 15] */}
        <div className="space-y-8">
          <div className="bg-gray-50 p-6 rounded-3xl border border-gray-100">
            <h3 className="font-bold text-lg mb-4 flex items-center gap-2">🕒 Giờ hoạt động</h3>
            <div className="flex justify-between text-gray-700">
              <span>Mở cửa: <b>{shop.openTime}</b></span>
              <span>Đóng cửa: <b>{shop.closeTime}</b></span>
            </div>
            <div className="mt-4 pt-4 border-t border-gray-200">
              <p className="text-sm text-gray-500">📍 Vị trí: {shop.floor} - Lô {shop.locationSlot}</p>
            </div>
          </div>

          <div className="space-y-4">
            <h3 className="font-bold text-lg flex items-center gap-2">🎟️ Ưu đãi độc quyền</h3>
            {shop.vouchers.map(v => (
              <div key={v.id} className="bg-red-50 border-2 border-dashed border-red-200 p-4 rounded-2xl relative overflow-hidden">
                <div className="absolute -left-2 top-1/2 -translate-y-1/2 w-4 h-4 bg-white rounded-full"></div>
                <div className="absolute -right-2 top-1/2 -translate-y-1/2 w-4 h-4 bg-white rounded-full"></div>
                <p className="text-red-600 font-black text-lg">{v.title}</p>
                <code className="bg-red-100 px-2 rounded text-red-800 text-xs font-bold uppercase">{v.code}</code>
                <p className="text-gray-500 text-xs mt-2">HSD: {v.validUntil}</p>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
};