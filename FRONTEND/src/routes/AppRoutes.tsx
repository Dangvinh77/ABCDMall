import { Routes, Route, Navigate } from "react-router-dom";
import { MoviesRoutes } from "../features/movies/routes/MovieRoutes";
import { MoviesAdminRoutes } from "../features/movies-admin/routes/MoviesAdminRoutes";
import { MapPage } from "../pages/directory/MapPage"; // Gọi từ pages
import { ShopDetailPage } from "../pages/shops/ShopDetailPage"; // Gọi từ pages

export function AppRoutes() {
  return (
    <Routes>
      <Route path="/" element={<Navigate to="/map" replace />} />
      
      {/* Các route của team Movies */}
      <Route path="/movies/*" element={<MoviesRoutes />} />
      <Route path="/movies/admin/*" element={<MoviesAdminRoutes />} />
      
      {/* Route Bản đồ */}
      <Route path="/map" element={<MapPage />} />

      {/* Route Chi tiết Shop - Phải để TRƯỚC dấu sao */}
      <Route path="/shops/:slug" element={<ShopDetailPage />} />

      {/* CATCH-ALL (404) - Luôn để ở cuối cùng */}
      <Route path="*" element={<div className="p-20 text-center">404 Not Found</div>} />
    </Routes>
  );
}