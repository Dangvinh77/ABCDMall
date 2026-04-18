import { Navigate, Route, Routes } from "react-router-dom";
import AdminManagement from "../features/auth/pages/AdminManagement";
import Dashboard from "../features/auth/pages/Dashboard";
import ForgotPassword from "../features/auth/pages/ForgotPassword";
import Login from "../features/auth/pages/Login";
import Profile from "../features/auth/pages/Profile";
import Register from "../features/auth/pages/Register";
import RentalAreasAdmin from "../features/auth/pages/RentalAreasAdmin";
import RevenueStatistics from "../features/auth/pages/RevenueStatistics";
import ShopInfo from "../features/auth/pages/ShopInfo";
import UserManagement from "../features/auth/pages/UserManagement";
import FoodDetailPage from "../features/food/pages/FoodDetailPage";
import FoodPage from "../features/food/pages/FoodPage";
import { MoviesAdminRoutes } from "../features/movies-admin/routes/MoviesAdminRoutes";
import { MoviesRoutes } from "../features/movies/routes/MovieRoutes";
import { HomePage } from "../pages/home/HomePage";

export function AppRoutes() {
  return (
    <Routes>
      <Route path="/" element={<HomePage />} />
      <Route path="/login" element={<Login />} />
      <Route path="/dashboard" element={<Dashboard />} />
      <Route path="/profile" element={<Profile />} />
      <Route path="/forgot-password" element={<ForgotPassword />} />
      <Route path="/register" element={<Register />} />
      <Route path="/shop-info" element={<ShopInfo />} />
      <Route path="/admin-management" element={<AdminManagement />} />
      <Route path="/admin-management/users" element={<UserManagement />} />
      <Route path="/admin-management/revenue" element={<RevenueStatistics />} />
      <Route path="/rental-areas" element={<RentalAreasAdmin />} />
      <Route path="/users" element={<Navigate to="/admin-management/users" replace />} />
      <Route path="/revenue-statistics" element={<Navigate to="/admin-management/revenue" replace />} />

      <Route path="/food-court" element={<FoodPage />} />
      <Route path="/food/:slug" element={<FoodDetailPage />} />
      <Route path="/mall/:mall/:slug" element={<FoodDetailPage />} />

      <Route path="/movies/*" element={<MoviesRoutes />} />
      <Route path="/movies/admin/*" element={<MoviesAdminRoutes />} />

      <Route
        path="/shops"
        element={
          <div className="min-h-[50vh] p-20 text-center text-2xl font-bold">
            Trang Cua hang (Sap ra mat)
          </div>
        }
      />
      <Route
        path="/gallery"
        element={
          <div className="min-h-[50vh] p-20 text-center text-2xl font-bold">
            Trang Thu vien (Sap ra mat)
          </div>
        }
      />
      <Route
        path="/map"
        element={
          <div className="min-h-[50vh] p-20 text-center text-2xl font-bold">
            Trang So do (Sap ra mat)
          </div>
        }
      />
      <Route
        path="/contact"
        element={
          <div className="min-h-[50vh] p-20 text-center text-2xl font-bold">
            Trang Lien he (Sap ra mat)
          </div>
        }
      />

      <Route path="*" element={<div>404 Not Found</div>} />
    </Routes>
  );
}
