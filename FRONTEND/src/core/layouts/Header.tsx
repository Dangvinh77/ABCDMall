import { useState, useEffect } from 'react';
import { NavLink, Link } from 'react-router-dom';

export const Header = () => {
  const [isVisible, setIsVisible] = useState(true);
  const [lastScrollY, setLastScrollY] = useState(0);

  useEffect(() => {
    const handleScroll = () => {
      const currentScrollY = window.scrollY;
      if (currentScrollY > lastScrollY && currentScrollY > 50) {
        setIsVisible(false);
      } else {
        setIsVisible(true);
      }
      setLastScrollY(currentScrollY);
    };

    window.addEventListener('scroll', handleScroll);
    return () => window.removeEventListener('scroll', handleScroll);
  }, [lastScrollY]);

  const navClass = ({ isActive }: { isActive: boolean }) => 
    `font-bold text-sm uppercase tracking-wide transition-colors duration-300 ${
      isActive ? 'text-red-500' : 'text-gray-600 hover:text-red-500'
    }`;

  // Các danh mục thương hiệu (Lấy từ MapDataSeeder)
  const brandCategories = [
    { name: 'Thời Trang', slug: 'thoi-trang' },
    { name: 'Trang Sức & Phụ Kiện', slug: 'phu-kien' },
    { name: 'Ẩm Thực', slug: 'am-thuc' },
    { name: 'Sức Khỏe & Làm Đẹp', slug: 'lam-dep' },
    { name: 'Nhà Sách & Giáo Dục', slug: 'giao-duc' }
  ];

  return (
    <header className={`fixed top-0 left-0 w-full z-50 bg-white/95 backdrop-blur-md shadow-sm border-b border-gray-100 transition-transform duration-300 ${isVisible ? 'translate-y-0' : '-translate-y-full'}`}>
      <div className="max-w-7xl mx-auto px-6 h-20 flex items-center justify-between">
        
        {/* LOGO */}
        <Link to="/" className="flex items-center gap-2 group">
          <div className="w-10 h-10 bg-gradient-to-br from-red-500 to-orange-500 text-white rounded-xl flex items-center justify-center font-black text-xl shadow-md group-hover:scale-105 transition-transform">
            A
          </div>
          <span className="font-black text-2xl tracking-tighter text-gray-800">
            ABCD<span className="text-red-500">MALL</span>
          </span>
        </Link>

        {/* MENU NAV */}
        <nav className="hidden md:flex items-center gap-8 h-full">
          <NavLink to="/" className={navClass}>Trang Chủ</NavLink>
          <NavLink to="/map" className={navClass}>Sơ đồ Mall</NavLink>
          
          {/* MENU DROPDOWN "THƯƠNG HIỆU" */}
          <div className="relative group h-full flex items-center">
            <NavLink to="/brands" className={navClass}>
              Thương Hiệu <span className="ml-1 text-[10px]">▼</span>
            </NavLink>
            
            {/* Dropdown Panel */}
            <div className="absolute top-full left-1/2 -translate-x-1/2 w-56 bg-white shadow-xl rounded-2xl border border-gray-100 opacity-0 invisible group-hover:opacity-100 group-hover:visible transition-all duration-300 translate-y-2 group-hover:translate-y-0">
              <div className="p-2 flex flex-col">
                <Link to="/brands" className="px-4 py-3 hover:bg-red-50 hover:text-red-600 font-semibold text-gray-700 rounded-xl transition-colors border-b border-gray-50 mb-1">
                  Tất cả Thương Hiệu
                </Link>
                {brandCategories.map(cat => (
                  <Link 
                    key={cat.slug} 
                    to={`/brands?category=${cat.slug}`} 
                    className="px-4 py-2 hover:bg-gray-50 text-gray-600 text-sm font-medium rounded-lg transition-colors"
                  >
                    {cat.name}
                  </Link>
                ))}
              </div>
            </div>
          </div>

          <NavLink to="/movies" className={navClass}>Rạp Phim</NavLink>
          <NavLink to="/gallery" className={navClass}>Thư Viện</NavLink>
        </nav>

        {/* RIGHT ACTIONS */}
        <div className="flex items-center gap-4">
          <button className="w-10 h-10 rounded-full bg-gray-100 hover:bg-gray-200 flex items-center justify-center transition-colors">
            🔍
          </button>
        </div>
      </div>
    </header>
  );
};