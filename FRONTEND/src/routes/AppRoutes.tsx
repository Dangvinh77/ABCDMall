import { Routes, Route, Navigate } from 'react-router-dom'
import { MoviesRoutes } from '../features/movies/routes/MovieRoutes'
export function AppRoutes() {
  return (
      <Routes>
        <Route path="/" element={<Navigate to="/movies" replace />} />
          <Route path="/movies/*" element={<MoviesRoutes />} />
          {/* Thay Route dưới bằng một element riêng cho trang notfound */}
      <Route path="*" element={<div>404 Not Found</div>} /> 
    </Routes>
  )
}
