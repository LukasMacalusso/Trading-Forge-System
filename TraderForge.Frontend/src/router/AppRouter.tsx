import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AppLayout } from '@components/Layouts/AppLayout';
import { AuthLayout } from '@components/Layouts/AuthLayout';
import { ProtectedRoute } from './ProtectedRoute';
import { AdminRoute } from './AdminRoute';
import { LandingPage } from '@pages/Landing/LandingPage';
import { DashboardPage } from '@pages/Dashboard/DashboardPage';
import { PortfolioPage } from '@pages/Portfolio/PortfolioPage';
import { AllAssetsPage } from '@pages/Market/AllAssetsPage';
import { MyStrategiesPage } from '@pages/Bots/MyStrategiesPage';
import { StrategyBuilderPage } from '@pages/Bots/StrategyBuilderPage';
import { AccountPage } from '@pages/Account/AccountPage';
import { AdminDashboardPage } from '@pages/Admin/AdminDashboardPage';
import { LoginPage } from '@pages/Auth/LoginPage';
import { RegisterPage } from '@pages/Auth/RegisterPage';

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
            <Route path="/strategy" element={<MyStrategiesPage />} />
            <Route path="/strategy/:id" element={<StrategyBuilderPage />} />
            <Route path="/cuenta" element={<AccountPage />} />
            <Route path="/pending" element={<div className="p-6 text-neutral-400">Pending Operations — coming soon</div>} />
            <Route path="/subscription" element={<div className="p-6 text-neutral-400">Subscription — coming soon</div>} />
          </Route>
        </Route>

        {/* Admin area — requires SystemAdmin role */}
        <Route element={<AdminRoute />}>
          <Route path="/admin" element={<AdminDashboardPage />} />
        </Route>

        {/* Fallback */}
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </BrowserRouter>
  );
}
