import { Navigate, Route, Routes } from "react-router-dom";
import FoodDetailPage from "../pages/FoodDetailPage";
import FoodPage from "../pages/FoodPage";

export function FoodRoutes() {
  return (
    <Routes>
      <Route index element={<FoodPage />} />
      <Route path=":slug" element={<FoodDetailPage />} />
      <Route path="*" element={<Navigate to="/food-court" replace />} />
    </Routes>
  );
}
