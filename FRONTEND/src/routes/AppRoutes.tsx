import { Routes, Route } from "react-router-dom";
import { FaqPage } from "../pages/support/FaqPage";
import { FeedbackPage } from "../pages/feedbacks/FeedbackPage";
import { ContactPage } from "../pages/contact/ContactPage";
import { HomePage } from "../pages/home/HomePage";
import { MapPage } from "../pages/directory/MapPage";
import { ShopDetailPage } from "../pages/shops/ShopDetailPage";
import FoodPage from "../features/food/pages/FoodPage";
import FoodDetailPage from "../features/food/pages/FoodDetailPage";
import { BrandsPage } from "../pages/brands/BrandsPage";

// Team Movies Routes
import { MoviesRoutes } from "../features/movies/routes/MovieRoutes";
import { MoviesAdminRoutes } from "../features/movies-admin/routes/MoviesAdminRoutes";

export function AppRoutes() {
  return (
    <Routes>
      {/* 1. Trang chủ */}
      <Route path="/" element={<HomePage />} />

      {/* 2. Bản đồ & Chỉ đường */}
      <Route path="/map" element={<MapPage />} />

      <Route path="/shops/:slug" element={<ShopDetailPage />} />
      <Route path="/contact" element={<ContactPage />} />

      {/* 4. Food Court (Chuyển từ App.tsx cũ sang) */}
      <Route path="/food-court" element={<FoodPage />} />
      <Route path="/food/:slug" element={<FoodDetailPage />} />
      <Route path="/mall/:mall/:slug" element={<FoodDetailPage />} />

      {/* 5. Team Movies (Sử dụng sub-routes) */}
      <Route path="/movies/*" element={<MoviesRoutes />} />
      <Route path="/movies/admin/*" element={<MoviesAdminRoutes />} />

      {/* 6. Placeholder cho các trang khác */}
      <Route path="/gallery" element={<div className="p-20 text-center">Gallery Page</div>} />
      <Route path="/contact" element={<div className="p-20 text-center">Contact Page</div>} />

      <Route path="/faq" element={<FaqPage />} />
      <Route path="/feedback" element={<FeedbackPage />} />

      {/* Route Mới cho Thương Hiệu */}
      <Route path="/brands" element={<BrandsPage />} />

      {/* 7. Catch-all (404) - Luôn ở cuối cùng */}
      <Route path="*" element={<div className="p-20 text-center text-2xl font-bold">404 Not Found</div>} />
    </Routes>
  );
}