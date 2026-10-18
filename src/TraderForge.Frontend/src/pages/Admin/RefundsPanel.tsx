import { Check, Receipt, X } from 'lucide-react';
import type { RefundRequest, RefundStatus } from '@models/Admin';
import { Badge } from '@components/UI/Badge';
import { Button } from '@components/UI/Button';

interface RefundsPanelProps {
  refunds: RefundRequest[];
  busyId: string | null;
  onResolve: (refund: RefundRequest, action: 'approve' | 'reject') => void;
}

const statusLabel: Record<RefundStatus, { label: string; variant: 'up' | 'down' | 'warning' }> = {
  Pending: { label: 'Pendiente', variant: 'warning' },
  Approved: { label: 'Aprobado', variant: 'up' },
  Rejected: { label: 'Rechazado', variant: 'down' },
};

function formatDate(iso: string): string {
  return new Date(iso).toLocaleDateString('es-ES', { day: '2-digit', month: 'short', year: 'numeric' });
}

export function RefundsPanel({ refunds, busyId, onResolve }: RefundsPanelProps) {
  if (refunds.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center text-center gap-3 py-20 text-neutral-600">
        <Receipt size={36} className="text-neutral-700" />
        <p className="text-sm text-neutral-400">No hay solicitudes de reembolso</p>
      </div>
    );
  }

  function handleResolve(refund: RefundRequest, action: 'approve' | 'reject') {
    const message =
      action === 'approve'
        ? `¿Aprobar el reembolso de $${refund.amount} a ${refund.userEmail}?`
        : `¿Rechazar la solicitud de reembolso de ${refund.userEmail}?`;
    if (window.confirm(message)) onResolve(refund, action);
  }

  return (
    <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
      {refunds.map((refund) => {
        const status = statusLabel[refund.status];
        const isPending = refund.status === 'Pending';
        return (
          <div key={refund.id} className="bg-neutral-900 border border-neutral-800 rounded-xl p-5 flex flex-col gap-3">
            <div className="flex items-start justify-between gap-3">
              <div className="flex flex-col min-w-0">
                <span className="text-sm font-medium text-neutral-100 truncate">{refund.userEmail}</span>
                <span className="text-xs text-neutral-500">
                  Plan {refund.plan} · {formatDate(refund.requestedAt)}
                </span>
              </div>
              <span className="text-lg font-bold text-amber-400 font-mono shrink-0">
                ${refund.amount.toLocaleString('en-US', { minimumFractionDigits: 2 })}
              </span>
            </div>

            <p className="text-sm text-neutral-400 bg-neutral-950/60 border border-neutral-800 rounded-lg p-3">
              {refund.reason}
            </p>

            <div className="flex items-center justify-between gap-2">
              <Badge variant={status.variant}>{status.label}</Badge>
              {isPending && (
                <div className="flex items-center gap-2">
                  <Button
                    variant="danger"
                    size="sm"
                    isLoading={busyId === refund.id}
                    onClick={() => handleResolve(refund, 'reject')}
                  >
                    <X size={14} />
                    Rechazar
                  </Button>
                  <Button
                    variant="primary"
                    size="sm"
                    className="!bg-emerald-500 hover:!bg-emerald-400"
                    isLoading={busyId === refund.id}
                    onClick={() => handleResolve(refund, 'approve')}
                  >
                    <Check size={14} />
                    Aprobar
                  </Button>
                </div>
              )}
            </div>
          </div>
        );
      })}
    </div>
  );
}
