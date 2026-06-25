import { useEffect, useState } from 'react';
import { usePortfolioStore } from '@store/portfolioStore';
import { SubscriptionService } from '@api/SubscriptionService';
import type { PlanInfo } from '@api/SubscriptionService';

const subscriptionService = new SubscriptionService();

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
  const { portfolio } = usePortfolioStore();
  const [plan, setPlan] = useState<PlanInfo | null>(null);
  const [planLoading, setPlanLoading] = useState(true);

  useEffect(() => {
    subscriptionService.getMyPlan().then((result) => {
      if (result.isSuccess) setPlan(result.value!);
      setPlanLoading(false);
    });
  }, []);

  const activeAssets = portfolio?.positions.length ?? 0;
  const activeStrategies = 0;

  return (
    <div className="flex flex-col h-full overflow-y-auto scrollbar-thin p-6 gap-6">
      <div>
        <h2 className="text-xl font-bold text-neutral-100">Cuenta</h2>
        <p className="text-sm text-neutral-500 mt-0.5">Tu plan y límites de uso</p>
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
    </div>
  );
}
