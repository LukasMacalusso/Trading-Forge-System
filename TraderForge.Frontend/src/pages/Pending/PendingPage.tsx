import { CheckCircle2 } from 'lucide-react';
import { usePendingOperations } from '@hooks/usePendingOperations';
import { PendingOperationCard } from '@components/Notifications/PendingOperationCard';

export function PendingPage() {
  const { pendingOperations, approve, reject, expire, pendingId } = usePendingOperations();
  const count = pendingOperations.length;

  return (
    <div className="flex flex-col h-full overflow-y-auto scrollbar-thin p-6 gap-6">
      <div>
        <h2 className="text-xl font-bold text-neutral-100">Operaciones pendientes</h2>
        <p className="text-sm text-neutral-500 mt-0.5">
          Acciones de tus bots que esperan tu autorización antes de ejecutarse
        </p>
      </div>

      {count === 0 ? (
        <div className="flex-1 flex flex-col items-center justify-center text-center gap-3 text-neutral-600">
          <CheckCircle2 size={40} className="text-neutral-700" />
          <div>
            <p className="text-sm text-neutral-400">No hay operaciones pendientes</p>
            <p className="text-xs text-neutral-600 mt-1">
              Cuando un bot de acción requiera aprobación manual, aparecerá aquí con un temporizador de 15 minutos.
            </p>
          </div>
        </div>
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-3 gap-4">
          {pendingOperations.map((op) => (
            <PendingOperationCard
              key={op.id}
              operation={op}
              isBusy={pendingId === op.id}
              onApprove={approve}
              onReject={reject}
              onExpire={expire}
            />
          ))}
        </div>
      )}
    </div>
  );
}
