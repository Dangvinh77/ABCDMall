import { Navigate, Outlet, useLocation } from "react-router-dom";

export function MoviesAdminRouteGuard() {
  const location = useLocation();
  const token = localStorage.getItem("token");
  const role = localStorage.getItem("role");

  if (!token) {
    return <Navigate to="/login" replace state={{ from: location.pathname }} />;
  }

  if (role !== "MoviesAdmin" && role !== "Admin") {
    return <Navigate to="/" replace />;
  }

  return <Outlet />;
}
