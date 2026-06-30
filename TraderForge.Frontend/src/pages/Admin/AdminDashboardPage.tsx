import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Layers, LogOut, Receipt, ShieldCheck, Users } from 'lucide-react';
import { useAdmin } from '@hooks/useAdmin';
import { useAdminPlans } from '@hooks/useAdminPlans';
import { useAuthStore } from '@store/authStore';
import { decodeJwt } from '@utils/jwt';
import { UsersPanel } from './UsersPanel';
import { RefundsPanel } from './RefundsPanel';
import { PlansPanel } from './PlansPanel';

type Tab = 'users' | 'plans' | 'refunds';

export function AdminDashboardPage() {
  const { users, refunds, isLoading, busyId, toggleUserStatus, resolveRefund } = useAdmin();
  const {
    plans,
    isLoading: plansLoading,
    busyId: plansBusyId,
    createPlan,
    updatePlan,
    deletePlan,
  } = useAdminPlans();
  const token = useAuthStore((s) => s.token);
  const logout = useAuthStore((s) => s.logout);
  const navigate = useNavigate();
  const [tab, setTab] = useState<Tab>('users');

  const adminEmail = decodeJwt(token)?.email ?? 'Administrador';
  const pendingRefunds = refunds.filter((r) => r.status === 'Pending').length;

  function handleLogout() {
    logout();
    navigate('/');
  }

  const tabs: { id: Tab; label: string; icon: typeof Users; badge?: number }[] = [
    { id: 'users', label: 'Usuarios', icon: Users, badge: users.length || undefined },
    { id: 'plans', label: 'Planes', icon: Layers, badge: plans.length || undefined },
    { id: 'refunds', label: 'Reembolsos', icon: Receipt, badge: pendingRefunds || undefined },
  ];

  return (
    <div className="flex flex-col h-screen bg-neutral-950">
      {/* Top bar */}
      <header className="shrink-0 flex items-center justify-between px-6 py-4 border-b border-neutral-800">
        <div className="flex items-center gap-3">
          <span className="w-9 h-9 rounded-lg bg-amber-500/10 text-amber-400 flex items-center justify-center">
            <ShieldCheck size={18} />
          </span>
          <div>
            <h1 className="text-base font-bold text-neutral-100 leading-tight">
              Trading <span className="text-amber-400">Forge</span> · Admin
            </h1>
            <p className="text-xs text-neutral-500">{adminEmail}</p>
          </div>
        </div>
        <button
          onClick={handleLogout}
          className="flex items-center gap-2 px-3 py-2 rounded-lg text-sm text-neutral-400 hover:text-neutral-100 hover:bg-neutral-800 transition-colors"
        >
          <LogOut size={16} />
          Salir
        </button>
      </header>

      {/* Tabs */}
      <div className="shrink-0 flex items-center gap-1 px-6 pt-4 border-b border-neutral-800">
        {tabs.map(({ id, label, icon: Icon, badge }) => {
          const isActive = tab === id;
          return (
            <button
              key={id}
              onClick={() => setTab(id)}
              className={`flex items-center gap-2 px-4 py-2.5 text-sm rounded-t-lg border-b-2 -mb-px transition-colors ${
                isActive
                  ? 'border-amber-400 text-amber-400 font-medium'
                  : 'border-transparent text-neutral-500 hover:text-neutral-200'
              }`}
            >
              <Icon size={15} />
              {label}
              {badge != null && (
                <span
                  className={`min-w-5 h-5 px-1.5 flex items-center justify-center rounded-full text-[11px] font-bold ${
                    isActive ? 'bg-amber-500/20 text-amber-300' : 'bg-neutral-800 text-neutral-400'
                  }`}
                >
                  {badge}
                </span>
              )}
            </button>
          );
        })}
      </div>

      {/* Content */}
      <main className="flex-1 overflow-y-auto scrollbar-thin p-6">
        {tab === 'plans' ? (
          plansLoading ? (
            <div className="flex items-center justify-center h-full text-neutral-600 text-sm animate-pulse">
              Cargando...
            </div>
          ) : (
            <PlansPanel
              plans={plans}
              busyId={plansBusyId}
              onCreate={createPlan}
              onUpdate={updatePlan}
              onDelete={deletePlan}
            />
          )
        ) : isLoading ? (
          <div className="flex items-center justify-center h-full text-neutral-600 text-sm animate-pulse">
            Cargando...
          </div>
        ) : tab === 'users' ? (
          <UsersPanel users={users} busyId={busyId} onToggleStatus={toggleUserStatus} />
        ) : (
          <RefundsPanel refunds={refunds} busyId={busyId} onResolve={resolveRefund} />
        )}
      </main>
    </div>
  );
}
