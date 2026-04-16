import { useParams, Link } from 'react-router-dom';
import { useState, useEffect } from 'react';
import { ShopDetail } from './types/shop.types';
import { getImageUrl } from "@/core/utils/image";

export const ShopDetailFeature = () => {
  const { slug } = useParams<{ slug: string }>();
  const [shop, setShop] = useState<ShopDetail | null>(null);
  const [loading, setLoading] = useState<boolean>(true); // Thêm state loading
  const [error, setError] = useState<string | null>(null); // Thêm state báo lỗi

  // MOCK DATA Thương hiệu tương tự (Giữ nguyên hoặc sửa lại sau)
  const similarBrands: any[] = [ /* ... */ ];

  useEffect(() => {
    const fetchShopData = async () => {
      if (!slug) return;
      
      setLoading(true);
      setError(null);
      
      try {
        const response = await fetch(`http://localhost:5184/api/shops/${slug.toLowerCase()}`);
       
        if (!response.ok) {
          throw new Error('Không tìm thấy cửa hàng');
        }
        
        const data = await response.json();
        setShop(data);
        
      } catch (err: any) {
        console.error("Lỗi khi gọi API:", err);
        setError(err.message);
      } finally {
        setLoading(false);
      }
      
    };

    fetchShopData();
    window.scrollTo(0, 0);
  }, [slug]);

  // HIỂN THỊ LOADING HOẶC LỖI
  if (loading) return (
    <div className="min-h-screen flex flex-col items-center justify-center gap-4">
      <div className="animate-spin text-5xl">⏳</div>
      <p className="text-gray-500 font-medium">Đang tải thông tin cửa hàng...</p>
    </div>
  );

  if (error || !shop) return (
    <div className="min-h-screen flex flex-col items-center justify-center gap-4">
      <div className="text-5xl">🏪</div>
      <p className="text-red-500 font-bold text-xl">{error || 'Không tìm thấy cửa hàng!'}</p>
      <Link to="/directory" className="px-6 py-2 bg-gray-900 text-white rounded-full hover:bg-gray-800">Quay lại Bản đồ</Link>
    </div>
  );

  const featuredProducts = shop.products?.filter(p => p.isFeatured) || [];
  const discountedProducts = shop.products?.filter(p => p.isDiscounted) || [];

  return (
    <div className="bg-slate-50 min-h-screen pb-20">
      
      {/* 1. HERO BANNER - HIỆU ỨNG PARALLAX NHẸ */}
      <div className="relative h-[450px] w-full overflow-hidden group">
        <img 
          //src={shop.coverImageUrl} 
          src={getImageUrl(shop.coverImageUrl)}
  
          alt="Cover" 

         className="w-full h-full object-contain bg-black transform group-hover:scale-105 transition-transform duration-1000" 
        
  
        />
        <div className="absolute inset-0 bg-gradient-to-t from-black/90 via-black/40 to-transparent" />
        
        <div className="absolute bottom-0 left-1/2 -translate-x-1/2 translate-y-10 w-full px-6 xl:px-0 max-w-[1400px] flex flex-col md:flex-row items-end gap-8 pb-10 z-10">
          {/* Logo Shop */}
          <div className="w-36 h-36 bg-white rounded-[2rem] p-3 shadow-2xl border-4 border-white shrink-0 hover:-translate-y-2 transition-transform duration-300">
            <img 
            //src={shop.logoUrl}
            src={getImageUrl(shop.logoUrl)}
            
            className="w-full h-full object-contain" alt="Logo" />

            
          </div>
          <div className="text-white pb-2 flex-1">
            <h1 className="text-5xl md:text-6xl font-black uppercase tracking-tight drop-shadow-lg">{shop.name}</h1>
            <p className="text-xl font-medium opacity-90 italic drop-shadow mt-2">"{shop.slogan}"</p>
          </div>
        </div>
      </div>

      {/* KHUNG NỘI DUNG CHÍNH (MAX-W-1400PX) */}
      <div className="max-w-[1400px] mx-auto px-6 pt-24 space-y-16">
        
        {/* 2. INFO BAR - Thông tin cửa hàng */}
        <div className="bg-white rounded-3xl p-6 shadow-sm border border-gray-100 flex flex-col md:flex-row justify-between items-center gap-6">
          <p className="text-gray-600 text-lg leading-relaxed flex-1">
            {shop.description}
          </p>
          <div className="flex gap-4 shrink-0 bg-gray-50 p-4 rounded-2xl border border-gray-100">
            <div className="text-center px-4 border-r border-gray-200">
              <p className="text-xs text-gray-400 font-bold uppercase mb-1">Giờ mở cửa</p>
              <p className="text-lg font-black text-gray-800">{shop.openTime} - {shop.closeTime}</p>
            </div>
            <div className="text-center px-4">
              <p className="text-xs text-gray-400 font-bold uppercase mb-1">Vị trí</p>
              <p className="text-lg font-black text-red-500">{shop.floor} • Lô {shop.locationSlot}</p>
            </div>
          </div>
        </div>

        {/* 3. SẢN PHẨM TIÊU BIỂU (LƯỚI 5 CỘT) */}
        {featuredProducts.length > 0 && (
          <div>
            <div className="flex items-center gap-3 mb-8">
              <span className="w-2 h-8 bg-gray-800 rounded-full"></span>
              <h2 className="text-3xl font-black text-gray-800">Sản Phẩm Tiêu Biểu</h2>
            </div>
            
            <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-5 gap-6">
              {featuredProducts.map(p => (
                <div key={p.id} className="bg-white rounded-2xl p-3 hover:shadow-xl hover:-translate-y-2 transition-all duration-300 border border-gray-100 group">
                  <div className="aspect-[4/5] rounded-xl overflow-hidden mb-4 relative bg-gray-50">
                    <img 
                    //src={p.imageUrl}
                    src={getImageUrl(p.imageUrl)}
                     className="w-full h-full object-cover group-hover:scale-110 transition-transform duration-500" alt={p.name} />
                  </div>
                  <h3 className="font-bold text-gray-800 line-clamp-2 min-h-[3rem] mb-2">{p.name}</h3>
                  <p className="text-gray-900 font-black text-xl">{p.price.toLocaleString()} ₫</p>
                </div>
              ))}
            </div>
          </div>
        )}

        {/* 4. SĂN DEAL GIẢM GIÁ */}
        {discountedProducts.length > 0 && (
          <div>
            <div className="flex items-center gap-3 mb-8">
              <span className="w-2 h-8 bg-red-500 rounded-full"></span>
              <h2 className="text-3xl font-black text-red-600 flex items-center gap-2">
                Săn Deal Giảm Giá 🔥
              </h2>
            </div>
            
            <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-5 gap-6">
              {discountedProducts.map(p => (
                <div key={p.id} className="bg-white rounded-2xl p-3 hover:shadow-[0_8px_30px_rgb(239,68,68,0.15)] hover:-translate-y-2 transition-all duration-300 border-2 border-transparent hover:border-red-100 group relative">
                  <div className="absolute top-5 right-5 z-10 bg-red-500 text-white font-black text-sm px-3 py-1 rounded-full shadow-md">
                    -{p.discountPercent}%
                  </div>

                  <div className="aspect-[4/5] rounded-xl overflow-hidden mb-4 relative bg-gray-50">
                    <img
                    //src={p.imageUrl} 
                    src={getImageUrl(p.imageUrl)}
                    className="w-full h-full object-cover group-hover:scale-110 transition-transform duration-500" alt={p.name}/>
                  </div>
                  <h3 className="font-bold text-gray-800 line-clamp-2 min-h-[3rem] mb-2">{p.name}</h3>
                  <div className="flex items-end gap-2">
                    <p className="text-red-600 font-black text-xl">{p.price.toLocaleString()} ₫</p>
                    {p.oldPrice && (
                      <p className="text-gray-400 line-through text-sm font-semibold mb-0.5">{p.oldPrice.toLocaleString()} ₫</p>
                    )}
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}

        {/* 5. VOUCHER / MÃ GIẢM GIÁ */}
        {shop.vouchers && shop.vouchers.length > 0 && (
          <div className="pt-10 border-t border-gray-200">
            <h2 className="text-2xl font-black text-gray-800 mb-6 text-center">🎟️ Lưu Mã Ưu Đãi Ngay</h2>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
              {shop.vouchers.map(v => (
                <div key={v.id} className="relative flex bg-gradient-to-r from-red-500 to-orange-500 rounded-2xl p-0.5 shadow-lg hover:-translate-y-1 transition-transform cursor-pointer">
                  <div className="flex w-full bg-white rounded-[0.9rem] overflow-hidden">
                    <div className="bg-red-50 text-red-600 flex flex-col justify-center items-center px-6 py-4 border-r-2 border-dashed border-red-200 font-black text-2xl relative">
                      <div className="absolute -left-2 top-1/2 -translate-y-1/2 w-4 h-4 bg-slate-50 rounded-full border-r border-red-200" />
                      {v.code}
                    </div>
                    <div className="p-4 flex-1">
                      <h3 className="font-bold text-gray-800 text-lg">{v.title}</h3>
                      <p className="text-sm text-gray-500 mt-1 mb-3">{v.description}</p>
                      <span className="text-xs font-bold bg-gray-100 text-gray-500 px-2 py-1 rounded">HSD: {v.validUntil}</span>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}

        {/* 6. THƯƠNG HIỆU BẠN CÓ THỂ THÍCH */}
        <div className="pt-16 pb-8 border-t border-gray-200">
          <div className="flex items-center justify-between mb-8">
            <div className="flex items-center gap-3">
              <span className="w-2 h-8 bg-gradient-to-b from-red-500 to-orange-500 rounded-full"></span>
              <h2 className="text-3xl font-black text-gray-800">Thương Hiệu Tương Tự</h2>
            </div>
            <Link to="/brands" className="text-red-500 font-bold hover:underline flex items-center gap-1">
              Xem tất cả <span className="text-xl">→</span>
            </Link>
          </div>

          <div className="grid grid-cols-2 md:grid-cols-4 gap-6">
            {similarBrands.map(brand => (
              <Link 
                to={`/shops/${brand.slug}`} 
                key={brand.id}
                className="bg-white rounded-[2rem] border border-gray-100 p-6 flex flex-col items-center justify-center text-center shadow-sm hover:shadow-xl hover:-translate-y-2 transition-all duration-300 group relative overflow-hidden"
              >
                {/* Thanh màu dưới đáy khi hover */}
                <div className="absolute bottom-0 left-0 w-full h-1 bg-gradient-to-r from-red-500 to-orange-400 opacity-0 group-hover:opacity-100 transition-opacity"></div>

                <div className="w-20 h-20 md:w-28 md:h-28 mb-4 p-2">
                  <img 
                   // src={brand.logo} 
                    src={getImageUrl(brand.logoUrl)}
                    alt={brand.name} 
                    className="w-full h-full object-contain grayscale group-hover:grayscale-0 transition-all duration-500" 
                  />
                </div>
                <h3 className="font-black text-gray-800 uppercase tracking-wide group-hover:text-red-500 transition-colors">{brand.name}</h3>
                <p className="text-xs font-bold text-gray-400 mt-2 bg-gray-50 px-3 py-1 rounded-full">📍 {brand.floor}</p>
              </Link>
            ))}
          </div>
        </div>

      </div>
    </div>
  );
};