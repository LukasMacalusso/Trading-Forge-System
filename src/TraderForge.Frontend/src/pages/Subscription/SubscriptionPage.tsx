import { useEffect, useState } from 'react';
import { motion } from 'framer-motion';
import { Check, Zap, Infinity as InfinityIcon, Ban, Sparkles } from 'lucide-react';
import { SubscriptionService } from '@api/SubscriptionService';
import type { PlanInfo, RetentionOffer } from '@api/SubscriptionService';
import { useNotificationStore } from '@store/notificationStore';
import { Badge } from '@components/UI/Badge';
import { Button } from '@components/UI/Button';
import { Input } from '@components/UI/Input';
import { ConfirmDialog } from '@components/UI/ConfirmDialog';

const subscriptionService = new SubscriptionService();

function planFeatures(plan: PlanInfo): { label: string; unlimited: boolean }[] {
  return [
    {
      label: `Capital virtual inicial: $${plan.initialVirtualBalance.toLocaleString()}`,
      unlimited: false,
    },
    {
      label:
        plan.maxActiveStrategies == null
          ? 'Estrategias ilimitadas'
          : `Hasta ${plan.maxActiveStrategies} estrategias simultáneas`,
      unlimited: plan.maxActiveStrategies == null,
    },
    {
      label:
        plan.maxActiveAssets == null
          ? 'Activos ilimitados'
          : `Hasta ${plan.maxActiveAssets} activos en portfolio`,
      unlimited: plan.maxActiveAssets == null,
    },
    ...(plan.canModifyVirtualBalance
      ? [{ label: 'Saldo virtual modificable', unlimited: false }]
      : []),
  ];
}

export function SubscriptionPage() {
  const addNotification = useNotificationStore((s) => s.addNotification);

  const [plans, setPlans] = useState<PlanInfo[]>([]);
  const [currentPlan, setCurrentPlan] = useState<PlanInfo | null>(null);
  const [loading, setLoading] = useState(true);

  const [promo, setPromo] = useState('');
  const [target, setTarget] = useState<PlanInfo | null>(null);
  const [changing, setChanging] = useState(false);

  const [cancelOpen, setCancelOpen] = useState(false);
  const [retentionOffer, setRetentionOffer] = useState<RetentionOffer | null>(null);
  const [cancelling, setCancelling] = useState(false);

  useEffect(() => {
    Promise.all([subscriptionService.getPlans(), subscriptionService.getMyPlan()]).then(
      ([plansResult, planResult]) => {
        if (plansResult.isSuccess) {
          setPlans([...plansResult.value!].sort((a, b) => a.monthlyPrice - b.monthlyPrice));
        } else {
          addNotification('error', plansResult.errorMessage ?? 'No se pudieron cargar los planes.');
        }
        if (planResult.isSuccess) setCurrentPlan(planResult.value!);
        setLoading(false);
      },
    );
  }, [addNotification]);

  async function handleConfirmChange() {
    if (!target) return;
    setChanging(true);
    const result = await subscriptionService.changePlan(target.id, promo.trim() || undefined);
    setChanging(false);

    if (!result.isSuccess) {
      addNotification('error', result.errorMessage ?? 'No se pudo cambiar el plan.');
      return;
    }

    addNotification('success', `Plan actualizado a ${target.name}`);
    setCurrentPlan(target);
    setTarget(null);
    setPromo('');
  }

  async function handleCancel(force: boolean) {
    setCancelling(true);
    const result = await subscriptionService.cancel(force);
    setCancelling(false);

    if (!result.isSuccess) {
      addNotification('error', result.errorMessage ?? 'No se pudo cancelar la suscripción.');
      return;
    }

    // The backend held off cancelling to present a retention offer.
    if (result.value!.requiresForceCancel && result.value!.discount) {
      setCancelOpen(false);
      setRetentionOffer(result.value!.discount);
      return;
    }

    setCancelOpen(false);
    setRetentionOffer(null);
    addNotification('success', 'Suscripción cancelada.');
    const refreshed = await subscriptionService.getMyPlan();
    if (refreshed.isSuccess) setCurrentPlan(refreshed.value!);
  }

  return (
    <div className="flex flex-col h-full overflow-y-auto scrollbar-thin p-6 gap-6">
      <div className="flex items-end justify-between gap-4 flex-wrap">
        <div>
          <h2 className="text-xl font-bold text-neutral-100">Plan y suscripción</h2>
          <p className="text-sm text-neutral-500 mt-0.5">
            Cambia de plan en cualquier momento. El cambio se aplica al instante.
          </p>
        </div>
        {currentPlan && !loading && (
          <Badge variant="warning">Plan actual: {currentPlan.name}</Badge>
        )}
      </div>

      {/* Promo code */}
      <div className="max-w-sm">
        <Input
          label="Código promocional (opcional)"
          placeholder="Ej. WELCOME20"
          value={promo}
          onChange={(e) => setPromo(e.target.value.toUpperCase())}
        />
      </div>

      {loading ? (
        <div className="grid gap-5 md:grid-cols-3">
          {[0, 1, 2].map((i) => (
            <div
              key={i}
              className="h-72 rounded-2xl border border-neutral-800 bg-neutral-900/40 animate-pulse"
            />
          ))}
        </div>
      ) : (
        <div className="grid gap-5 md:grid-cols-3 items-start">
          {plans.map((plan, idx) => {
            const isCurrent = currentPlan?.id === plan.id;
            const features = planFeatures(plan);

            return (
              <motion.div
                key={plan.id}
                initial={{ opacity: 0, y: 16 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ duration: 0.4, delay: idx * 0.08 }}
                className={`relative rounded-2xl p-6 flex flex-col gap-5 ${
                  isCurrent
                    ? 'bg-neutral-900 border border-amber-500/40 shadow-[0_0_50px_rgba(245,158,11,0.07)]'
                    : 'bg-neutral-900/50 border border-neutral-800/70'
                }`}
              >
                {isCurrent && (
                  <div className="absolute -top-3 left-1/2 -translate-x-1/2">
                    <span className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full bg-amber-500 text-neutral-950 text-[11px] font-bold uppercase tracking-wide shadow-lg shadow-amber-500/30">
                      <Zap size={10} strokeWidth={2.5} />
                      Tu plan
                    </span>
                  </div>
                )}

                <div>
                  <p
                    className={`text-xs font-semibold uppercase tracking-[0.12em] mb-3 ${
                      isCurrent ? 'text-amber-400' : 'text-neutral-500'
                    }`}
                  >
                    {plan.name}
                  </p>
                  <div className="flex items-baseline gap-1">
                    <span className="text-4xl font-black text-neutral-100 tracking-tight">
                      ${plan.monthlyPrice}
                    </span>
                    <span className="text-sm text-neutral-500">/mes</span>
                  </div>
                </div>

                <div className="h-px bg-neutral-800/60" />

                <ul className="flex flex-col gap-3 flex-1">
                  {features.map((f) => (
                    <li key={f.label} className="flex items-start gap-2.5">
                      {f.unlimited ? (
                        <InfinityIcon size={15} strokeWidth={2.5} className="mt-0.5 shrink-0 text-amber-400" />
                      ) : (
                        <Check size={14} strokeWidth={2.5} className="mt-0.5 shrink-0 text-amber-500/70" />
                      )}
                      <span className="text-sm text-neutral-300">{f.label}</span>
                    </li>
                  ))}
                </ul>

                <Button
                  variant={isCurrent ? 'secondary' : 'primary'}
                  disabled={isCurrent}
                  onClick={() => setTarget(plan)}
                >
                  {isCurrent ? 'Plan actual' : `Cambiar a ${plan.name}`}
                </Button>
              </motion.div>
            );
          })}
        </div>
      )}

      {/* Cancel subscription */}
      {!loading && currentPlan && (
        <div className="mt-2 rounded-xl border border-red-500/20 bg-red-500/5 p-4 flex items-center justify-between gap-4 flex-wrap">
          <div>
            <p className="text-sm font-medium text-neutral-200">Cancelar suscripción</p>
            <p className="text-xs text-neutral-500 mt-0.5">
              Tu plan dejará de renovarse y volverás al acceso básico.
            </p>
          </div>
          <Button variant="danger" size="sm" onClick={() => setCancelOpen(true)}>
            <Ban size={14} />
            Cancelar suscripción
          </Button>
        </div>
      )}

      <ConfirmDialog
        isOpen={target != null}
        icon={<Zap size={20} />}
        title={target ? `¿Cambiar al plan ${target.name}?` : ''}
        description={
          target
            ? `Tu suscripción pasará a ${target.name} por $${target.monthlyPrice}/mes${
                promo.trim() ? ` con el código ${promo.trim()}` : ''
              }. El cambio se aplica de inmediato.`
            : ''
        }
        confirmLabel="Confirmar cambio"
        isLoading={changing}
        onConfirm={handleConfirmChange}
        onClose={() => setTarget(null)}
      />

      {/* Cancel — initial confirmation */}
      <ConfirmDialog
        isOpen={cancelOpen}
        variant="danger"
        icon={<Ban size={20} />}
        title="¿Cancelar tu suscripción?"
        description="Tu plan dejará de renovarse y perderás sus beneficios."
        confirmLabel="Sí, cancelar"
        cancelLabel="Volver"
        isLoading={cancelling}
        onConfirm={() => handleCancel(false)}
        onClose={() => setCancelOpen(false)}
      />

      {/* Cancel — retention offer */}
      <ConfirmDialog
        isOpen={retentionOffer != null}
        variant="danger"
        icon={<Sparkles size={20} />}
        title="¡Espera! Tenemos una oferta para ti"
        description={
          retentionOffer
            ? `Quédate con un ${retentionOffer.percentage}% de descuento: pagarías solo $${retentionOffer.discountedPrice.toFixed(2)}/mes. ¿Aún quieres cancelar?`
            : ''
        }
        confirmLabel="Cancelar de todas formas"
        cancelLabel="Mantener mi plan"
        isLoading={cancelling}
        onConfirm={() => handleCancel(true)}
        onClose={() => setRetentionOffer(null)}
      />
    </div>
  );
}
