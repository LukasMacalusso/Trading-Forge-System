import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AppLayout } from '../Layouts/AppLayout';
import { AuthLayout } from '../Layouts/AuthLayout';
import { ProtectedRoute } from './ProtectedRoute';
import { LandingPage } from '../Pages/Landing/LandingPage';
import { DashboardPage } from '../Pages/Dashboard/DashboardPage';
import { PortfolioPage } from '../Pages/Portfolio/PortfolioPage';
import { AllAssetsPage } from '../Pages/Market/AllAssetsPage';
import { AccountPage } from '../Pages/Account/AccountPage';
import { LoginPage } from '../Pages/Auth/LoginPage';
import { RegisterPage } from '../Pages/Auth/RegisterPage';

export function AppRouter() {
  return (
    <BrowserRouter>
      <Routes>
        {/* Public landing */}
        <Route path="/" element={<LandingPage />} />

        {/* Auth pages */}
        <Route element={<AuthLayout />}>
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />
        </Route>

        {/* Protected app — requires authentication */}
        <Route element={<ProtectedRoute />}>
          <Route element={<AppLayout />}>
            <Route path="/dashboard" element={<DashboardPage />} />
            <Route path="/portfolio" element={<PortfolioPage />} />
            <Route path="/all" element={<AllAssetsPage />} />
            <Route path="/cuenta" element={<AccountPage />} />
            <Route path="/pending" element={<div className="p-6 text-neutral-400">Pending Operations — coming soon</div>} />
            <Route path="/subscription" element={<div className="p-6 text-neutral-400">Subscription — coming soon</div>} />
          </Route>
        </Route>

        {/* Fallback */}
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </BrowserRouter>
  );
}
