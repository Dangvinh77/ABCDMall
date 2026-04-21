import { Link, NavLink } from "react-router-dom";

export const Header = () => {
  const navClass = ({ isActive }: { isActive: boolean }) =>
    `font-bold text-[15px] uppercase tracking-wide transition-colors duration-300 ${
      isActive ? "text-red-600" : "text-gray-600 hover:text-red-500"
    }`;

  const brandCategories = [
    { name: "Thoi Trang", slug: "thoi-trang" },
    { name: "Trang Suc & Phu Kien", slug: "phu-kien" },
    { name: "Am Thuc", slug: "am-thuc" },
    { name: "Suc Khoe & Lam Dep", slug: "lam-dep" },
    { name: "Nha Sach & Giao Duc", slug: "giao-duc" },
  ];

  return (
    <header
      className="relative z-50 w-full border-b border-gray-100 bg-white/95 shadow-sm backdrop-blur-md"
    >
      <div className="mx-auto flex h-20 max-w-7xl items-center justify-between px-6">
        <Link to="/" className="group flex shrink-0 items-center gap-2">
          <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-gradient-to-br from-red-600 to-orange-500 text-xl font-black text-white shadow-md transition-transform group-hover:scale-105">
            A
          </div>
          <span className="text-2xl font-black tracking-tighter text-gray-800">
            ABCD<span className="text-red-600">MALL</span>
          </span>
        </Link>

        <nav className="hidden h-full flex-1 items-center justify-end gap-10 md:flex">
          <NavLink to="/" className={navClass}>
            Trang Chu
          </NavLink>
          <NavLink to="/map" className={navClass}>
            So do Mall
          </NavLink>

          <div className="group relative flex h-full items-center">
            <NavLink to="/brands" className={navClass}>
              Thuong Hieu <span className="ml-1 text-[10px]">▼</span>
            </NavLink>

            <div className="invisible absolute top-full left-1/2 w-64 -translate-x-1/2 translate-y-2 overflow-hidden rounded-2xl border border-gray-100 bg-white opacity-0 shadow-2xl transition-all duration-300 group-hover:visible group-hover:translate-y-0 group-hover:opacity-100">
              <div className="flex flex-col p-2">
                <Link
                  to="/brands"
                  className="mb-1 rounded-xl bg-red-50 px-4 py-3 text-center font-bold text-red-600 transition-colors"
                >
                  Tat ca Thuong Hieu
                </Link>
                {brandCategories.map((category) => (
                  <Link
                    key={category.slug}
                    to={`/brands?category=${category.slug}`}
                    className="rounded-xl px-4 py-2.5 text-sm font-semibold text-gray-600 transition-colors hover:bg-gray-50"
                  >
                    {category.name}
                  </Link>
                ))}
              </div>
            </div>
          </div>

          <NavLink to="/movies" className={navClass}>
            Rap Phim
          </NavLink>
          <NavLink to="/amenities" className={navClass}>
            Tien Ich
          </NavLink>
        </nav>
      </div>
    </header>
  );
};
