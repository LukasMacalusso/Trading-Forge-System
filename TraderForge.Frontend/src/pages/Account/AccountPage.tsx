import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Mail, RotateCcw, Trash2, AlertTriangle } from 'lucide-react';
import { usePortfolio } from '@hooks/usePortfolio';
import { useAuthStore } from '@store/authStore';
import { useNotificationStore } from '@store/notificationStore';
import { SubscriptionService } from '@api/SubscriptionService';
import { IdentityService } from '@api/IdentityService';
import type { PlanInfo } from '@api/SubscriptionService';
import { decodeJwt } from '@utils/jwt';
import { Badge } from '@components/UI/Badge';
import { Button } from '@components/UI/Button';
import { ConfirmDialog } from '@components/UI/ConfirmDialog';

const subscriptionService = new SubscriptionService();
const identityService = new IdentityService();

function UsageBar({ used, max, label }: { used: number; max: number | null; label: string }) {
  const percent = max != null ? Math.min((used / max) * 100, 100) : 0;
  const remaining = max != null ? max - used : null;
  const isWarning = max != null && used >= max * 0.8;

  return (
    <div className="flex flex-col gap-1.5">
      <div className="flex items-center justify-between text-xs">
        <span className="text-neutral-400">{label}</span>
        <span className={`font-mono font-semibold ${isWarning ? 'text-red-400' : 'text-neutral-200'}`}>
          {used} / {max ?? '∞'}
          {remaining != null && (
            <span className="text-neutral-600 font-normal ml-1">
              ({remaining} restante{remaining !== 1 ? 's' : ''})
            </span>
          )}
        </span>
      </div>
      {max != null && (
        <div className="h-1.5 bg-neutral-800 rounded-full overflow-hidden">
          <div
            className={`h-full rounded-full transition-all ${isWarning ? 'bg-red-500' : 'bg-amber-500'}`}
            style={{ width: `${percent}%` }}
          />
        </div>
      )}
    </div>
  );
}

export function AccountPage() {
  const { portfolio, resetSimulation } = usePortfolio();
  const token = useAuthStore((s) => s.token);
  const logout = useAuthStore((s) => s.logout);
  const addNotification = useNotificationStore((s) => s.addNotification);
  const navigate = useNavigate();

  const [plan, setPlan] = useState<PlanInfo | null>(null);
  const [planLoading, setPlanLoading] = useState(true);

  const [resetOpen, setResetOpen] = useState(false);
  const [resetting, setResetting] = useState(false);
  const [deleteOpen, setDeleteOpen] = useState(false);
  const [deleting, setDeleting] = useState(false);

  useEffect(() => {
    subscriptionService.getMyPlan().then((result) => {
      if (result.isSuccess) setPlan(result.value!);
      setPlanLoading(false);
    });
  }, []);

  const email = decodeJwt(token)?.email ?? '—';
  const avatarLetter = email.charAt(0).toUpperCase();
  const activeAssets = portfolio?.positions.length ?? 0;
  const activeStrategies = 0;

  async function handleReset() {
    setResetting(true);
    const ok = await resetSimulation();
    setResetting(false);
    if (ok) setResetOpen(false);
  }

  async function handleDelete() {
    setDeleting(true);
    const result = await identityService.deleteAccount();
    setDeleting(false);
    if (result.isSuccess) {
      setDeleteOpen(false);
      logout();
      navigate('/');
      return;
    }
    addNotification('error', result.errorMessage ?? 'No se pudo eliminar la cuenta.');
  }

  return (
    <div className="flex flex-col h-full overflow-y-auto scrollbar-thin p-6 gap-6">
      <div>
        <h2 className="text-xl font-bold text-neutral-100">Cuenta</h2>
        <p className="text-sm text-neutral-500 mt-0.5">Gestiona tu perfil, plan y configuración</p>
      </div>

      {/* Profile */}
      <div className="bg-neutral-900 border border-neutral-800 rounded-xl p-5 flex items-center gap-4">
        <div className="w-12 h-12 shrink-0 rounded-full bg-amber-500/10 text-amber-400 flex items-center justify-center text-lg font-bold">
          {avatarLetter}
        </div>
        <div className="flex flex-col gap-1 min-w-0">
          <div className="flex items-center gap-2 text-sm text-neutral-200 font-medium">
            <Mail size={14} className="text-neutral-500 shrink-0" />
            <span className="truncate">{email}</span>
          </div>
          <div>
            {planLoading ? (
              <span className="text-xs text-neutral-600 animate-pulse">Cargando plan...</span>
            ) : (
              <Badge variant="warning">{plan?.name ?? 'Sin plan'}</Badge>
            )}
          </div>
        </div>
      </div>

      {/* Plan info */}
      <div className="bg-neutral-900 border border-neutral-800 rounded-xl p-5 flex flex-col gap-1">
        <p className="text-xs text-neutral-500 uppercase tracking-wider">Plan actual</p>
        {planLoading ? (
          <p className="text-2xl font-bold text-neutral-600 animate-pulse">Cargando...</p>
        ) : (
          <>
            <p className="text-2xl font-bold text-amber-400">{plan?.name ?? '—'}</p>
            {plan && (
              <p className="text-xs text-neutral-600 mt-0.5">
                ${plan.monthlyPrice}/mes · Capital virtual inicial: ${plan.initialVirtualBalance.toLocaleString()}
              </p>
            )}
          </>
        )}
      </div>

      {/* Usage */}
      <div className="bg-neutral-900 border border-neutral-800 rounded-xl p-5 flex flex-col gap-5">
        <h3 className="text-sm font-semibold text-neutral-200">Uso actual</h3>
        <UsageBar
          label="Activos activos"
          used={activeAssets}
          max={plan?.maxActiveAssets ?? null}
        />
        <UsageBar
          label="Estrategias activas"
          used={activeStrategies}
          max={plan?.maxActiveStrategies ?? null}
        />
      </div>

      {/* Upgrade note */}
      <div className="bg-amber-500/5 border border-amber-500/20 rounded-xl p-4 text-xs text-neutral-400">
        Para aumentar tus límites, ve a{' '}
        <span className="text-amber-400 font-semibold">Plan</span>{' '}
        y cambia tu suscripción.
      </div>

      {/* Danger zone */}
      <div className="bg-neutral-900 border border-red-500/20 rounded-xl overflow-hidden">
        <div className="px-5 py-3 border-b border-red-500/15 flex items-center gap-2">
          <AlertTriangle size={14} className="text-red-400" />
          <h3 className="text-sm font-semibold text-red-400">Zona de peligro</h3>
        </div>

        <div className="divide-y divide-neutral-800">
          <div className="p-5 flex items-center justify-between gap-4 flex-wrap">
            <div className="flex flex-col gap-0.5 min-w-0">
              <span className="text-sm font-medium text-neutral-200">Reiniciar simulación</span>
              <span className="text-xs text-neutral-500">
                Cierra todas tus posiciones y restablece tu capital virtual inicial.
              </span>
            </div>
            <Button variant="secondary" size="sm" onClick={() => setResetOpen(true)}>
              <RotateCcw size={14} />
              Reiniciar
            </Button>
          </div>

          <div className="p-5 flex items-center justify-between gap-4 flex-wrap">
            <div className="flex flex-col gap-0.5 min-w-0">
              <span className="text-sm font-medium text-neutral-200">Eliminar cuenta</span>
              <span className="text-xs text-neutral-500">
                Borra permanentemente tu cuenta y todos tus datos. Esta acción no se puede deshacer.
              </span>
            </div>
            <Button variant="danger" size="sm" onClick={() => setDeleteOpen(true)}>
              <Trash2 size={14} />
              Eliminar cuenta
            </Button>
          </div>
        </div>
      </div>

      {/* Reset confirmation */}
      <ConfirmDialog
        isOpen={resetOpen}
        icon={<RotateCcw size={20} />}
        title="¿Reiniciar la simulación?"
        description="Se cerrarán todas tus posiciones abiertas y tu capital virtual volverá al valor inicial de tu plan. El historial no se verá afectado."
        confirmLabel="Reiniciar"
        isLoading={resetting}
        onConfirm={handleReset}
        onClose={() => setResetOpen(false)}
      />

      {/* Delete confirmation */}
      <ConfirmDialog
        isOpen={deleteOpen}
        variant="danger"
        icon={<Trash2 size={20} />}
        title="¿Eliminar tu cuenta?"
        description="Esta acción es permanente. Se eliminarán tu perfil, tu portafolio y todo tu historial. No podrás recuperar tus datos."
        requireText="ELIMINAR"
        confirmLabel="Eliminar cuenta"
        isLoading={deleting}
        onConfirm={handleDelete}
        onClose={() => setDeleteOpen(false)}
      />
    </div>
  );
}
