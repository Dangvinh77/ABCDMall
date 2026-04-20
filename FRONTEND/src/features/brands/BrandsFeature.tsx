import { useState, useMemo, useEffect } from 'react';
import { useSearchParams, Link } from 'react-router-dom';
import { getShops, type Shop } from '../shops/api/shopApi';
import { getImageUrl } from "@/core/utils/image";

export const BrandsFeature = () => {
  const [searchParams, setSearchParams] = useSearchParams();
  const currentCategory = searchParams.get('category') || 'all';
  const [searchTerm, setSearchTerm] = useState('');
  const [activeLetter, setActiveLetter] = useState<string | null>(null);
  const [activeFloor, setActiveFloor] = useState<string | null>(null);
  
  const [brands, setBrands] = useState<Shop[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".split("");

  // 1. GIỮ NGUYÊN BỘ LỌC TIẾNG VIỆT CỦA BẠN
  const categories = [
    { name: 'Tất cả', slug: 'all' },
    { name: 'Thời Trang', slug: 'thoi-trang' },
    { name: 'Trang Sức & Phụ Kiện', slug: 'phu-kien' },
    { name: 'Ẩm Thực', slug: 'am-thuc' },
    { name: 'Sức Khỏe & Làm Đẹp', slug: 'lam-dep' },
    { name: 'Nhà Sách & Giáo Dục', slug: 'giao-duc' },
  ];

  const floors = ["Tầng 1", "Tầng 2", "Tầng 3", "Tầng 4"];

  // 2. TỪ ĐIỂN MAPPING (Khớp slug Tiếng Việt với Category Tiếng Anh từ Backend)
  const categoryKeywords: Record<string, string[]> = {
    'thoi-trang': ['fashion', 'clothing', 'lifestyle', 'apparel', 'thời trang'],
    'phu-kien': ['jewelry', 'accessories', 'watch', 'shoes', 'bags', 'trang sức', 'phụ kiện'],
    'am-thuc': ['food', 'beverage', 'restaurant', 'cafe', 'bakery', 'ẩm thực'],
    'lam-dep': ['health', 'beauty', 'cosmetic', 'spa', 'sức khỏe', 'làm đẹp'],
    'giao-duc': ['education', 'book', 'entertainment', 'nhà sách', 'giáo dục', 'giải trí']
  };

  useEffect(() => {
    let active = true;
    
    const fetchBrands = async () => {
      setIsLoading(true);
      setError(null);
      try {
        const data = await getShops();
        if (active) {
          setBrands(data);
        }
      } catch (err: unknown) {
        if (active) {
          const errorMsg = err instanceof Error ? err.message : "Không thể tải danh sách thương hiệu";
          setError(errorMsg);
          console.error("Lỗi khi tải danh sách thương hiệu:", err);
        }
      } finally {
        if (active) {
          setIsLoading(false);
        }
      }
    };
    
    fetchBrands();
    window.scrollTo(0, 0);
    
    return () => {
      active = false;
    };
  }, []);

  // 3. LOGIC LỌC THÔNG MINH
  const filteredBrands = useMemo(() => {
    return brands.filter(brand => {
      // Lọc Category
      let matchCategory = false;
      if (currentCategory === 'all') {
        matchCategory = true;
      } else {
        const keywords = categoryKeywords[currentCategory] || [];
        const shopCat = brand.category.toLowerCase();
        matchCategory = keywords.some(kw => shopCat.includes(kw));
      }

      // Lọc Search theo Tên
      const matchSearch = brand.name.toLowerCase().includes(searchTerm.toLowerCase());
      
      // Lọc Chữ cái đầu
      const matchLetter = !activeLetter || brand.name.toUpperCase().startsWith(activeLetter);
      
      // Lọc Tầng (Khớp "Tầng 1" với "Floor 1" hoặc "Tầng 1" từ Backend)
      let matchFloor = true;
      if (activeFloor) {
        const floorNumber = activeFloor.replace(/\D/g, ''); // Trích xuất số 1, 2, 3...
        const loc = brand.location.toLowerCase();
        matchFloor = loc.includes(`floor ${floorNumber}`) || 
                     loc.includes(`tầng ${floorNumber}`) || 
                     loc.includes(`l${floorNumber}`);
      }
      
      return matchCategory && matchSearch && matchLetter && matchFloor;
    });
  }, [brands, currentCategory, searchTerm, activeLetter, activeFloor]);

  return (
    <div className="min-h-screen bg-slate-50 pt-10 pb-20">
      <div className="max-w-7xl mx-auto px-6">
        
        {/* HEADER */}
        <div className="text-center mb-12">
          <h1 className="text-4xl md:text-5xl font-black uppercase text-transparent bg-clip-text bg-gradient-to-r from-red-600 to-orange-500 mb-6 tracking-tight">
            {categories.find(c => c.slug === currentCategory)?.name || 'Thương Hiệu'}
          </h1>
          
          <div className="flex flex-col md:flex-row justify-center items-center gap-4 max-w-3xl mx-auto">
            <div className="relative w-full md:w-1/2">
              <input 
                type="text" 
                placeholder="Tìm tên thương hiệu..." 
                className="w-full pl-6 pr-12 py-3.5 rounded-full border-2 border-transparent shadow-md focus:outline-none focus:border-red-400 transition-all"
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
              />
              <span className="absolute right-5 top-1/2 -translate-y-1/2 text-gray-400 text-lg">🔍</span>
            </div>
            
            <select 
              className="w-full md:w-1/3 px-6 py-3.5 rounded-full border-2 border-transparent shadow-md bg-white font-bold text-gray-700 cursor-pointer focus:outline-none focus:border-red-400 transition-all appearance-none"
              value={currentCategory}
              onChange={(e) => setSearchParams({ category: e.target.value })}
            >
              {categories.map(cat => (
                <option key={cat.slug} value={cat.slug}>{cat.name}</option>
              ))}
            </select>
          </div>
        </div>

        <div className="flex flex-col lg:flex-row gap-8">
          
          {/* SIDEBAR */}
          <div className="lg:w-64 shrink-0 flex flex-col gap-6">
            
            {/* Box 1: Bảng Chữ Cái */}
            <div className="bg-white rounded-[2rem] p-5 shadow-lg border border-gray-100">
              <div className="flex items-center gap-2 mb-4 px-2">
                <div className="w-2 h-6 bg-gradient-to-b from-red-500 to-orange-400 rounded-full"></div>
                <h3 className="font-black text-gray-800 uppercase tracking-wide">Chữ Cái</h3>
              </div>
              <div className="grid grid-cols-5 gap-2">
                <button 
                  onClick={() => setActiveLetter(null)}
                  className={`col-span-5 py-2 text-sm font-bold rounded-xl transition-all ${!activeLetter ? 'bg-gradient-to-r from-red-500 to-orange-500 text-white shadow-md' : 'bg-gray-50 text-gray-500 hover:bg-gray-100'}`}
                >
                  Tất cả
                </button>
                {alphabet.map(letter => (
                  <button 
                    key={letter}
                    onClick={() => setActiveLetter(letter)}
                    className={`aspect-square flex items-center justify-center text-sm font-bold rounded-xl transition-all ${activeLetter === letter ? 'bg-gradient-to-br from-red-500 to-orange-500 text-white shadow-md scale-110' : 'bg-gray-50 text-gray-600 hover:bg-red-50 hover:text-red-500'}`}
                  >
                    {letter}
                  </button>
                ))}
              </div>
            </div>

            {/* Box 2: Tầng (Floor) */}
            <div className="bg-white rounded-[2rem] p-5 shadow-lg border border-gray-100">
               <div className="flex items-center gap-2 mb-4 px-2">
                <div className="w-2 h-6 bg-gradient-to-b from-red-500 to-orange-400 rounded-full"></div>
                <h3 className="font-black text-gray-800 uppercase tracking-wide">Vị Trí Tầng</h3>
              </div>
              <div className="flex flex-col gap-2">
                <button 
                  onClick={() => setActiveFloor(null)}
                  className={`py-3 px-4 text-sm font-bold rounded-xl transition-all text-left ${!activeFloor ? 'bg-gradient-to-r from-red-500 to-orange-500 text-white shadow-md' : 'bg-gray-50 text-gray-500 hover:bg-gray-100'}`}
                >
                  Tất cả các tầng
                </button>
                {floors.map(floor => (
                  <button 
                    key={floor}
                    onClick={() => setActiveFloor(floor)}
                    className={`py-3 px-4 text-sm font-bold rounded-xl transition-all text-left flex justify-between items-center ${activeFloor === floor ? 'bg-gradient-to-r from-red-500 to-orange-500 text-white shadow-md translate-x-1' : 'bg-gray-50 text-gray-600 hover:bg-red-50 hover:text-red-500'}`}
                  >
                    <span>{floor}</span>
                    <span className="text-[10px] opacity-70">📍</span>
                  </button>
                ))}
              </div>
            </div>
          </div>

          {/* MAIN CONTENT: LƯỚI LOGO THƯƠNG HIỆU */}
          <div className="flex-1">
            {error ? (
              <div className="bg-white rounded-[2rem] border-2 border-red-200 p-12 text-center flex flex-col items-center gap-4">
                <span className="text-6xl">⚠️</span>
                <h3 className="text-2xl font-bold text-red-600">Lỗi tải dữ liệu!</h3>
                <p className="text-gray-600 max-w-md">{error}</p>
                <button 
                  onClick={() => window.location.reload()}
                  className="mt-4 px-6 py-2 bg-red-500 text-white font-bold rounded-full hover:bg-red-600 transition-colors"
                >
                  Thử lại
                </button>
              </div>
            ) : isLoading ? (
              <div className="text-center py-20 text-2xl animate-pulse text-gray-400">Đang tải danh sách thương hiệu...</div>
            ) : filteredBrands.length > 0 ? (
              <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6">
                {filteredBrands.map(brand => (
                  <Link 
                    to={`/shops/${brand.slug}`} 
                    key={brand.id}
                    className="bg-white rounded-[2rem] border border-gray-100 shadow-sm hover:shadow-2xl hover:-translate-y-2 transition-all duration-300 group flex flex-col items-center p-6 text-center relative overflow-hidden"
                  >
                    <div className="absolute top-0 left-0 w-full h-1 bg-gradient-to-r from-red-500 to-orange-400 opacity-0 group-hover:opacity-100 transition-opacity"></div>
                    
                    <div className="w-24 h-24 md:w-32 md:h-32 mb-4 flex items-center justify-center p-2">
                      <img 
                         // Ưu tiên hiện Logo, nếu không có mới hiện ảnh phụ
                         src={getImageUrl(brand.logoUrl || brand.imageUrl)}
                         alt={brand.name} 
                         className="max-w-full max-h-full object-contain group-hover:scale-110 transition-transform duration-500" 
                      />
                    </div>
                    <h3 className="font-black text-gray-800 uppercase tracking-wide group-hover:text-red-500 transition-colors">{brand.name}</h3>
                    <p className="text-xs font-bold text-gray-400 mt-2 bg-gray-50 px-3 py-1 rounded-full text-ellipsis overflow-hidden whitespace-nowrap w-full">📍 {brand.location}</p>
                  </Link>
                ))}
              </div>
            ) : (
              <div className="bg-white rounded-[2rem] border-2 border-dashed border-gray-200 p-20 text-center flex flex-col items-center">
                <span className="text-6xl mb-4">🏬</span>
                <h3 className="text-2xl font-bold text-gray-400">Chưa có thương hiệu phù hợp!</h3>
                <button 
                  onClick={() => {setSearchTerm(''); setActiveFloor(null); setActiveLetter(null); setSearchParams({ category: 'all' })}}
                  className="mt-6 px-8 py-3 bg-red-50 text-red-500 font-bold rounded-full hover:bg-red-100 transition-colors shadow-sm"
                >
                  Xóa bộ lọc
                </button>
              </div>
            )}
          </div>

        </div>
      </div>
    </div>
  );
};