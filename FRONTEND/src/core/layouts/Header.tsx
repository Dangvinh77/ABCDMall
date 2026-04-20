import { useEffect, useState } from "react";
import { Link, NavLink } from "react-router-dom";
import { getImageUrl } from "../utils/image";

type UserProfile = {
  email?: string;
  role?: string;
  fullName?: string | null;
  image?: string | null;
};

const getStoredProfile = (): UserProfile | null => {
  const token = localStorage.getItem("token");
  if (!token) {
    return null;
  }

  const storedProfile = localStorage.getItem("profile");
  if (storedProfile) {
    try {
      return JSON.parse(storedProfile) as UserProfile;
    } catch {
      localStorage.removeItem("profile");
    }
  }

  const role = localStorage.getItem("role");
  return role ? { role } : null;
};

export const Header = () => {
  const [isVisible, setIsVisible] = useState(true);
  const [lastScrollY, setLastScrollY] = useState(0);
  const [profile, setProfile] = useState<UserProfile | null>(() => getStoredProfile());

  useEffect(() => {
    const handleScroll = () => {
      const currentScrollY = window.scrollY;
      setIsVisible(!(currentScrollY > lastScrollY && currentScrollY > 50));
      setLastScrollY(currentScrollY);
    };

    window.addEventListener("scroll", handleScroll);
    return () => window.removeEventListener("scroll", handleScroll);
  }, [lastScrollY]);

  useEffect(() => {
    const syncProfile = () => setProfile(getStoredProfile());

    window.addEventListener("storage", syncProfile);
    window.addEventListener("auth:changed", syncProfile);

    return () => {
      window.removeEventListener("storage", syncProfile);
      window.removeEventListener("auth:changed", syncProfile);
    };
  }, []);

  const navClass = ({ isActive }: { isActive: boolean }) =>
    `font-bold text-[15px] uppercase tracking-wide transition-colors duration-300 ${
      isActive ? "text-red-600" : "text-gray-600 hover:text-red-500"
    }`;

  const brandCategories = [
    { name: "thời trang", slug: "thoi-trang" },
    { name: "trang sức & phụ kiện", slug: "phu-kien" },
    { name: "ẩm thực", slug: "am-thuc" },
    { name: "sức khỏe & làm đẹp", slug: "lam-dep" },
    { name: "nhà sách & giáo dục", slug: "giao-duc" },
  ];

  const displayEmail = profile?.email || "Account";
  const avatarInitial = displayEmail.charAt(0).toUpperCase();
  const avatarUrl = profile?.image ? getImageUrl(profile.image) : "";

  return (
    <header
      className={`fixed top-0 left-0 z-50 w-full border-b border-gray-100 bg-white/95 shadow-sm backdrop-blur-md transition-transform duration-300 ${
        isVisible ? "translate-y-0" : "-translate-y-full"
      }`}
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
            Trang Chủ
          </NavLink>
          <NavLink to="/map" className={navClass}>
            Sơ đồ Mall
          </NavLink>

          <div className="group relative flex h-full items-center">
            <NavLink to="/brands" className={navClass}>
              Thương Hiệu <span className="ml-1 text-[10px]">▼</span>
            </NavLink>

            <div className="invisible absolute top-full left-1/2 w-64 -translate-x-1/2 translate-y-2 overflow-hidden rounded-2xl border border-gray-100 bg-white opacity-0 shadow-2xl transition-all duration-300 group-hover:visible group-hover:translate-y-0 group-hover:opacity-100">
              <div className="flex flex-col p-2">
                <Link
                  to="/brands"
                  className="mb-1 rounded-xl bg-red-50 px-4 py-3 text-center font-bold text-red-600 transition-colors"
                >
                  tất cả Thương Hiệu
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
            Rạp Phim
          </NavLink>
          <NavLink to="/amenities" className={navClass}>
            Tiện Ích
          </NavLink>
          {profile ? (
            <Link
              to="/dashboard"
              className="group flex max-w-[260px] items-center gap-3 rounded-full border border-red-100 bg-red-50/80 py-1.5 pr-4 pl-1.5 shadow-sm transition-all duration-300 hover:-translate-y-0.5 hover:border-red-200 hover:bg-white hover:shadow-lg hover:shadow-red-100"
            >
              <span className="flex h-10 w-10 shrink-0 items-center justify-center overflow-hidden rounded-full bg-gradient-to-br from-red-600 to-orange-500 text-sm font-black text-white shadow-md">
                {avatarUrl ? (
                  <img src={avatarUrl} alt={displayEmail} className="h-full w-full object-cover" />
                ) : (
                  avatarInitial
                )}
              </span>
              <span className="min-w-0 text-left leading-tight">
                <span className="block truncate text-sm font-black text-gray-800">{displayEmail}</span>
              </span>
            </Link>
          ) : (
            <NavLink
              to="/login"
              className={({ isActive }) =>
                `rounded-full px-5 py-2.5 text-sm font-black uppercase tracking-wide shadow-md transition-all duration-300 ${
                  isActive
                    ? "bg-red-700 text-white shadow-red-200"
                    : "bg-gradient-to-r from-red-600 to-orange-500 text-white shadow-red-200 hover:-translate-y-0.5 hover:shadow-lg hover:shadow-red-200"
                }`
              }
            >
              Login
            </NavLink>
          )}
        </nav>
      </div>
    </header>
  );
};
