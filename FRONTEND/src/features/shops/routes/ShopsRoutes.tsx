import { Navigate, Route, Routes } from "react-router-dom";
import ShopDetailPage from "../pages/ShopDetailPage";
import ShopPage from "../pages/ShopPage";

export function ShopsRoutes() {
  return (
    <Routes>
      <Route index element={<ShopPage />} />
      <Route path=":slug" element={<ShopDetailPage />} />
      <Route path="*" element={<Navigate to="/shops" replace />} />
    </Routes>
  );
}
