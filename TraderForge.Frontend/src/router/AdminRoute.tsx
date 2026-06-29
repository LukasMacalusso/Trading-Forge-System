import { Navigate, Outlet } from 'react-router-dom';
import { useAuthStore } from '@store/authStore';

/** Restricts nested routes to authenticated SystemAdmin users. */
export function AdminRoute() {
  const { isAuthenticated, role } = useAuthStore();
  if (!isAuthenticated) return <Navigate to="/login" replace />;
  if (role !== 'SystemAdmin') return <Navigate to="/dashboard" replace />;
  return <Outlet />;
}
