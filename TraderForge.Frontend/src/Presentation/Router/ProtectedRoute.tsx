import { Navigate, Outlet } from 'react-router-dom';
import { useAuthStore } from '../../Application/Store/authStore';

export function ProtectedRoute() {
  const { isAuthenticated } = useAuthStore();
  return isAuthenticated ? <Outlet /> : <Navigate to="/login" replace />;
}
