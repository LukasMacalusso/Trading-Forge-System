import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AppLayout } from '../Layouts/AppLayout';
import { AuthLayout } from '../Layouts/AuthLayout';
import { DashboardPage } from '../Pages/Dashboard/DashboardPage';
import { PortfolioPage } from '../Pages/Portfolio/PortfolioPage';
import { LoginPage } from '../Pages/Auth/LoginPage';
import { RegisterPage } from '../Pages/Auth/RegisterPage';
import { ProtectedView } from '../Components/ProtectedView';

export function AppRouter() {
  return (
    <BrowserRouter>
      <Routes>
        <Route element={<AuthLayout />}>
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />
        </Route>

        <Route element={<AppLayout />}>
          <Route path="/dashboard" element={<DashboardPage />} />
          <Route path="/portfolio" element={<ProtectedView><PortfolioPage /></ProtectedView>} />
          <Route path="/bots" element={<ProtectedView><div className="p-6 text-neutral-400">Bot Builder — coming soon</div></ProtectedView>} />
          <Route path="/pending" element={<ProtectedView><div className="p-6 text-neutral-400">Pending Operations — coming soon</div></ProtectedView>} />
          <Route path="/subscription" element={<ProtectedView><div className="p-6 text-neutral-400">Subscription — coming soon</div></ProtectedView>} />
        </Route>

        <Route path="*" element={<Navigate to="/dashboard" replace />} />
      </Routes>
    </BrowserRouter>
  );
}
