import { Navigate, Route, Routes } from 'react-router-dom';
import { MoviesAdminRouteGuard } from '../components/MoviesAdminRouteGuard';
import { MoviesAdminDashboardPage } from '../pages/MoviesAdminDashboardPage';
import { MoviesAdminBookingsPage } from '../pages/MoviesAdminBookingsPage';
import { MoviesAdminMoviesPage } from '../pages/MoviesAdminMoviesPage';
import { MoviesAdminPaymentsPage } from '../pages/MoviesAdminPaymentsPage';
import { MoviesAdminRevenuePage } from '../pages/MoviesAdminRevenuePage';
import { MoviesAdminSectionPage } from '../pages/MoviesAdminSectionPage';
import { MoviesAdminShell } from '../pages/MoviesAdminShell';
import { MoviesAdminShowtimesPage } from '../pages/MoviesAdminShowtimesPage';
import { MoviesAdminUsersPage } from '../pages/MoviesAdminUsersPage';
import { MoviesAdminEmailsPage } from '../pages/MoviesAdminEmailsPage';

export function MoviesAdminRoutes() {
  return (
    <Routes>
      <Route element={<MoviesAdminRouteGuard />}>
        <Route element={<MoviesAdminShell />}>
          <Route index element={<MoviesAdminDashboardPage />} />
          <Route path="movies" element={<MoviesAdminMoviesPage />} />
          <Route path="showtimes" element={<MoviesAdminShowtimesPage />} />
          <Route path="seats" element={<MoviesAdminSectionPage sectionId="seats" />} />
          <Route path="bookings" element={<MoviesAdminBookingsPage />} />
          <Route path="revenue" element={<MoviesAdminRevenuePage />} />
          <Route path="payments" element={<MoviesAdminPaymentsPage />} />
          <Route path="emails" element={<MoviesAdminEmailsPage />} />
          <Route path="guests" element={<MoviesAdminSectionPage sectionId="guests" />} />
          <Route path="settings" element={<MoviesAdminSectionPage sectionId="settings" />} />
          <Route path="promotions" element={<MoviesAdminSectionPage sectionId="promotions" />} />
          <Route path="logs" element={<MoviesAdminSectionPage sectionId="logs" />} />
          <Route path="users" element={<MoviesAdminUsersPage />} />
          <Route path="*" element={<Navigate to="/movies/admin" replace />} />
        </Route>
      </Route>
    </Routes>
  );
}
