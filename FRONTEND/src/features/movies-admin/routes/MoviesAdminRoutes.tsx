import { Navigate, Route, Routes } from 'react-router-dom';
import { MoviesAdminDashboardPage } from '../pages/MoviesAdminDashboardPage';
import { MoviesAdminSectionPage } from '../pages/MoviesAdminSectionPage';
import { MoviesAdminShell } from '../pages/MoviesAdminShell';

export function MoviesAdminRoutes() {
  return (
    <Routes>
      <Route element={<MoviesAdminShell />}>
        <Route index element={<MoviesAdminDashboardPage />} />
        <Route path="movies" element={<MoviesAdminSectionPage sectionId="movies" />} />
        <Route path="showtimes" element={<MoviesAdminSectionPage sectionId="showtimes" />} />
        <Route path="seats" element={<MoviesAdminSectionPage sectionId="seats" />} />
        <Route path="bookings" element={<MoviesAdminSectionPage sectionId="bookings" />} />
        <Route path="payments" element={<MoviesAdminSectionPage sectionId="payments" />} />
        <Route path="emails" element={<MoviesAdminSectionPage sectionId="emails" />} />
        <Route path="guests" element={<MoviesAdminSectionPage sectionId="guests" />} />
        <Route path="settings" element={<MoviesAdminSectionPage sectionId="settings" />} />
        <Route path="promotions" element={<MoviesAdminSectionPage sectionId="promotions" />} />
        <Route path="logs" element={<MoviesAdminSectionPage sectionId="logs" />} />
        <Route path="users" element={<MoviesAdminSectionPage sectionId="users" />} />
        <Route path="*" element={<Navigate to="/admin/movies" replace />} />
      </Route>
    </Routes>
  );
}
