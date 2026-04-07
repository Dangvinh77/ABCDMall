import { Routes, Route, Navigate } from "react-router-dom";
import { MoviesAdminRoutes } from "../features/movies-admin/routes/MoviesAdminRoutes";
import { MoviesRoutes } from "../features/movies/routes/MovieRoutes";

export function AppRoutes() {
  return (
    <Routes>
      <Route path="/" element={<Navigate to="/movies" replace />} />
      <Route path="/movies/*" element={<MoviesRoutes />} />
      <Route path="/movies/admin/*" element={<MoviesAdminRoutes />} />
      <Route path="*" element={<div>404 Not Found</div>} />
    </Routes>
  );
}
