import { Navigate, Route, Routes } from 'react-router-dom';
import { AdminShell } from '../pages/AdminShell';
import { AdminDashboardPage } from '../pages/AdminDashboardPage';
import { UserManagementPage } from '../pages/UserManagementPage';
import { MapSlotManagementPage } from '../pages/MapSlotManagementPage';

export function AdminRoutes() {
  return (
    <Routes>
      <Route element={<AdminShell />}>
        <Route index element={<AdminDashboardPage />} />
        <Route path="dashboard" element={<AdminDashboardPage />} />
        <Route path="users" element={<UserManagementPage />} />
        <Route path="maps" element={<MapSlotManagementPage />} />
        <Route path="*" element={<Navigate to="/admin" replace />} />
      </Route>
    </Routes>
  );
}
