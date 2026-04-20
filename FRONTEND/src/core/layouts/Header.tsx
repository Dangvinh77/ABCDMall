import { useState, useEffect } from "react";
import { Link, NavLink } from "react-router-dom";

export const Header = () => {
  const [isVisible, setIsVisible] = useState(true);
  const [lastScrollY, setLastScrollY] = useState(0);

  useEffect(() => {
    const handleScroll = () => {
      const currentScrollY = window.scrollY;
      setIsVisible(!(currentScrollY > lastScrollY && currentScrollY > 50));
      setLastScrollY(currentScrollY);
    };

    window.addEventListener("scroll", handleScroll);
    return () => window.removeEventListener("scroll", handleScroll);
  }, [lastScrollY]);

  const navClass = ({ isActive }: { isActive: boolean }) =>
    `font-bold text-[15px] uppercase tracking-wider transition-all duration-300 relative py-2 ${
      isActive ? "text-red-600" : "text-gray-600 hover:text-red-500"
    }`;

  const brandCategories = [
    { name: "Thời Trang", slug: "thoi-trang" },
    { name: "Trang Sức & Phụ Kiện", slug: "phu-kien" },
    { name: "Ẩm Thực", slug: "am-thuc" },
    { name: "Sức Khỏe & Làm Đẹp", slug: "lam-dep" },
    { name: "Nhà Sách & Giáo Dục", slug: "giao-duc" },
  ];

  // SỬA LẠI ĐƯỜNG DẪN Ở ĐÂY CHO ĐÚNG THAM SỐ STATUS
  const eventTypes = [
    { name: "Đang diễn ra", path: "/events?status=Ongoing" },
    { name: "Sắp diễn ra", path: "/events?status=Upcoming" },
    { name: "Tất cả sự kiện", path: "/events" },
  ];

  return (
    <header
      className={`fixed top-0 left-0 z-50 w-full border-b border-gray-100 bg-white/95 shadow-sm backdrop-blur-md transition-transform duration-500 ${
        isVisible ? "translate-y-0" : "-translate-y-full"
      }`}
    >
      <div className="mx-auto flex h-20 w-full max-w-[1600px] items-center justify-between px-6 md:px-12">
        
        {/* LOGO */}
        <Link to="/" className="group flex shrink-0 items-center gap-3">
          <div className="flex h-11 w-11 items-center justify-center rounded-xl bg-gradient-to-br from-red-600 to-orange-500 text-2xl font-black text-white shadow-lg transition-transform group-hover:rotate-6 group-hover:scale-110">
            A
          </div>
          <div className="flex flex-col">
            <span className="text-2xl font-black leading-none tracking-tighter text-gray-800">
              ABCD<span className="text-red-600">MALL</span>
            </span>
            <span className="text-[10px] font-bold uppercase tracking-[0.2em] text-gray-400">Premium Shopping</span>
          </div>
        </Link>

        {/* MENU NAV */}
        <nav className="hidden h-full flex-1 items-center justify-center gap-x-8 lg:gap-x-12 md:flex ml-10">
          <NavLink to="/" className={navClass}>Trang Chủ</NavLink>
          <NavLink to="/map" className={navClass}>Sơ đồ Mall</NavLink>

          {/* DROPDOWN THƯƠNG HIỆU */}
          <div className="group relative flex h-full items-center">
            <NavLink to="/brands" className={navClass}>
              Thương Hiệu <span className="ml-1 text-[10px] opacity-50">▼</span>
            </NavLink>
            <div className="invisible absolute top-full left-1/2 w-60 -translate-x-1/2 translate-y-4 overflow-hidden rounded-2xl border border-gray-100 bg-white opacity-0 shadow-2xl transition-all duration-300 group-hover:visible group-hover:translate-y-0 group-hover:opacity-100">
              <div className="flex flex-col p-2">
                <Link to="/brands" className="mb-1 rounded-xl bg-orange-50 px-4 py-3 text-center font-bold text-orange-600 transition-colors hover:bg-orange-100">
                  Tất Cả Thương Hiệu
                </Link>
                {brandCategories.map((item) => (
                  <Link
                    key={item.slug}
                    to={`/brands?category=${item.slug}`}
                    className="rounded-xl px-4 py-2.5 text-sm font-semibold text-gray-600 transition-colors hover:bg-gray-50 hover:text-orange-500"
                  >
                    {item.name}
                  </Link>
                ))}
              </div>
            </div>
          </div>

          {/* DROPDOWN SỰ KIỆN */}
          <div className="group relative flex h-full items-center">
            <NavLink to="/events" className={navClass}>
              Sự Kiện <span className="ml-1 text-[10px] opacity-50">▼</span>
            </NavLink>
            <div className="invisible absolute top-full left-1/2 w-60 -translate-x-1/2 translate-y-4 overflow-hidden rounded-2xl border border-gray-100 bg-white opacity-0 shadow-2xl transition-all duration-300 group-hover:visible group-hover:translate-y-0 group-hover:opacity-100">
              <div className="flex flex-col p-2">
                <Link to="/events" className="mb-1 rounded-xl bg-orange-50 px-4 py-3 text-center font-bold text-orange-600 transition-colors hover:bg-orange-100">
                  Lịch Sự Kiện
                </Link>
                {eventTypes.map((type) => (
                  <Link
                    key={type.name}
                    to={type.path}
                    className="rounded-xl px-4 py-2.5 text-sm font-semibold text-gray-600 transition-colors hover:bg-gray-50 hover:text-orange-500"
                  >
                    {type.name}
                  </Link>
                ))}
              </div>
            </div>
          </div>

          <NavLink to="/movies" className={navClass}>Rạp Phim</NavLink>
          <NavLink to="/amenities" className={navClass}>Tiện Ích</NavLink>
        </nav>

        {/* NÚT LIÊN HỆ */}
        <div className="hidden lg:flex items-center">
          <Link to="/contact" className="px-6 py-2.5 bg-gray-900 text-white rounded-full text-xs font-bold uppercase tracking-widest hover:bg-red-600 transition-all shadow-md hover:shadow-red-200">
            Liên Hệ
          </Link>
        </div>
      </div>
    </header>
  );
};