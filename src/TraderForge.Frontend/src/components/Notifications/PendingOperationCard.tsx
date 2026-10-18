import { useMemo } from 'react';
import { ArrowDownRight, ArrowUpRight, Check, Clock, X } from 'lucide-react';
import type { PendingOperation } from '@models/BotFlow';
import { useCountdown } from '@hooks/useCountdown';
import { Badge } from '@components/UI/Badge';
import { Button } from '@components/UI/Button';

interface PendingOperationCardProps {
  operation: PendingOperation;
  /** True while this card's approve/reject request is in flight. */
  isBusy?: boolean;
  onApprove: (id: string) => void;
  onReject: (id: string) => void;
  onExpire: (id: string) => void;
}

/**
 * A single bot action awaiting manual authorization (FR-14). Shows the trade
 * intent, a live 15-minute countdown, and approve/reject actions.
 */
export function PendingOperationCard({
  operation,
  isBusy = false,
  onApprove,
  onReject,
  onExpire,
}: PendingOperationCardProps) {
  const { label, remainingMs, isExpired } = useCountdown(operation.expiresAt, () =>
    onExpire(operation.id),
  );

  const totalWindowMs = useMemo(() => {
    const start = new Date(operation.conditionMetAt).getTime();
    const end = new Date(operation.expiresAt).getTime();
    return Math.max(end - start, 1);
  }, [operation.conditionMetAt, operation.expiresAt]);

  const progress = Math.min((remainingMs / totalWindowMs) * 100, 100);
  const isUrgent = remainingMs <= 60_000; // under a minute

  const isBuy = operation.action === 'Buy';
  const total = operation.quantity * operation.currentPrice;
  const disabled = isBusy || isExpired;

  return (
    <div className="bg-neutral-900 border border-neutral-800 rounded-xl p-4 flex flex-col gap-3">
      {/* Header: flow + countdown */}
      <div className="flex items-center justify-between gap-2">
        <div className="flex items-center gap-2 min-w-0">
          <span
            className={`w-7 h-7 shrink-0 rounded-lg flex items-center justify-center ${
              isBuy ? 'bg-emerald-500/10 text-emerald-400' : 'bg-red-500/10 text-red-400'
            }`}
          >
            {isBuy ? <ArrowUpRight size={15} /> : <ArrowDownRight size={15} />}
          </span>
          <span className="text-sm font-medium text-neutral-200 truncate" title={operation.flowName}>
            {operation.flowName}
          </span>
        </div>
        <span
          className={`flex items-center gap-1 font-mono text-xs font-semibold tabular-nums ${
            isUrgent ? 'text-red-400' : 'text-neutral-400'
          }`}
          aria-label={`Tiempo restante ${label}`}
        >
          <Clock size={12} />
          {label}
        </span>
      </div>

      {/* Trade intent */}
      <div className="flex items-center justify-between gap-2 text-sm">
        <div className="flex items-center gap-2">
          <Badge variant={isBuy ? 'up' : 'down'}>{isBuy ? 'Comprar' : 'Vender'}</Badge>
          <span className="font-semibold text-neutral-100">{operation.symbol}</span>
        </div>
        <span className="font-mono text-neutral-400 text-xs">
          {operation.quantity} @ ${operation.currentPrice.toLocaleString('en-US', { minimumFractionDigits: 2 })}
        </span>
      </div>

      <div className="flex items-center justify-between text-xs">
        <span className="text-neutral-500">Total estimado</span>
        <span className="font-mono font-semibold text-neutral-200">
          ${total.toLocaleString('en-US', { minimumFractionDigits: 2 })}
        </span>
      </div>

      {/* Countdown bar */}
      <div className="h-1 bg-neutral-800 rounded-full overflow-hidden" role="progressbar" aria-valuenow={Math.round(progress)}>
        <div
          className={`h-full rounded-full transition-all duration-1000 ease-linear ${
            isUrgent ? 'bg-red-500' : 'bg-amber-500'
          }`}
          style={{ width: `${progress}%` }}
        />
      </div>

      {/* Actions */}
      <div className="flex items-center gap-2 mt-0.5">
        <Button
          variant="danger"
          size="sm"
          className="flex-1"
          disabled={disabled}
          isLoading={isBusy}
          onClick={() => onReject(operation.id)}
        >
          <X size={14} />
          Rechazar
        </Button>
        <Button
          variant="primary"
          size="sm"
          className="flex-1 !bg-emerald-500 hover:!bg-emerald-400"
          disabled={disabled}
          isLoading={isBusy}
          onClick={() => onApprove(operation.id)}
        >
          <Check size={14} />
          Aprobar
        </Button>
      </div>
    </div>
  );
}
