import { useEffect } from 'react';
import { Outlet, NavLink, useNavigate } from 'react-router-dom';
import { LayoutDashboard, Briefcase, CreditCard, Clock, LogIn, LogOut, Globe, User, Workflow } from 'lucide-react';
import { AlertBanner } from '@components/Notifications/AlertBanner';
import { OnboardingTour } from '@components/Onboarding/OnboardingTour';
import { HelpButton } from '@components/Onboarding/HelpButton';
import { useAuthStore } from '@store/authStore';
import { useOnboardingStore } from '@store/onboardingStore';
import { OnboardingRepository } from '@utils/OnboardingRepository';

const NAV_ITEMS: { to: string; icon: typeof LayoutDashboard; label: string; tour?: string }[] = [
  { to: '/dashboard', icon: LayoutDashboard, label: 'Dashboard' },
  { to: '/portfolio', icon: Briefcase, label: 'Portfolio' },
  { to: '/all', icon: Globe, label: 'All' },
  { to: '/strategy', icon: Workflow, label: 'Estrategias', tour: 'nav-strategy' },
  { to: '/pending', icon: Clock, label: 'Pending' },
  { to: '/subscription', icon: CreditCard, label: 'Plan' },
  { to: '/cuenta', icon: User, label: 'Cuenta' },
];

export function AppLayout() {
  const { isAuthenticated, logout } = useAuthStore();
  const startOnboarding = useOnboardingStore((s) => s.start);
  const navigate = useNavigate();

  function handleLogout() {
    logout();
    navigate('/');
  }

  // Auto-launch the tour once for first-time users.
  useEffect(() => {
    if (!isAuthenticated || OnboardingRepository.isCompleted()) return;
    const timer = setTimeout(startOnboarding, 700);
    return () => clearTimeout(timer);
  }, [isAuthenticated, startOnboarding]);

  return (
    <div className="flex h-screen bg-neutral-950 overflow-hidden">
      {/* Sidebar */}
      <aside className="w-56 shrink-0 flex flex-col bg-neutral-900 border-r border-neutral-800">
        <div className="px-4 py-5 border-b border-neutral-800">
          <h1 className="text-lg font-bold text-neutral-100 tracking-tight">
            Trading <span className="text-amber-400">Forge</span>
          </h1>
        </div>

        <nav data-tour="sidebar" className="flex-1 py-4 flex flex-col gap-1 px-2">
          {NAV_ITEMS.map(({ to, icon: Icon, label, tour }) => (
            <NavLink
              key={to}
              to={to}
              data-tour={tour}
              className={({ isActive }) =>
                `flex items-center gap-3 px-3 py-2 rounded-lg text-sm transition-colors ${
                  isActive
                    ? 'bg-amber-500/10 text-amber-400 font-medium'
                    : 'text-neutral-500 hover:text-neutral-200 hover:bg-neutral-800'
                }`
              }
            >
              <Icon size={16} />
              {label}
            </NavLink>
          ))}
        </nav>

        <div className="p-4 border-t border-neutral-800 shrink-0 flex flex-col gap-1">
          <HelpButton />
          {isAuthenticated ? (
            <button
              onClick={handleLogout}
              className="flex w-full items-center gap-3 px-3 py-2 rounded-lg text-sm transition-colors text-neutral-500 hover:text-neutral-200 hover:bg-neutral-800"
            >
              <LogOut size={16} />
              Log out
            </button>
          ) : (
            <NavLink
              to="/login"
              className="flex items-center gap-3 px-3 py-2 rounded-lg text-sm transition-colors text-neutral-500 hover:text-neutral-200 hover:bg-neutral-800"
            >
              <LogIn size={16} />
              Log in
            </NavLink>
          )}
        </div>
      </aside>

      {/* Main content */}
      <main className="flex-1 flex flex-col min-w-0 overflow-hidden">
        <Outlet />
      </main>

      <AlertBanner />
      <OnboardingTour />
    </div>
  );
}
