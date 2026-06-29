import { useCallback, useEffect, useState } from 'react';
import { useNotificationStore } from '@store/notificationStore';
import { PendingOperationsService } from '@api/PendingOperationsService';

const service = new PendingOperationsService();

// Ensures the one-time initial fetch runs once per app session, no matter how
// many components consume the hook (the notification center and the page both do).
let hasLoaded = false;

export function usePendingOperations() {
  const pendingOperations = useNotificationStore((s) => s.pendingOperations);
  const setPendingOperations = useNotificationStore((s) => s.setPendingOperations);
  const removePendingOperation = useNotificationStore((s) => s.removePendingOperation);
  const addNotification = useNotificationStore((s) => s.addNotification);

  const [pendingId, setPendingId] = useState<string | null>(null);

  useEffect(() => {
    if (hasLoaded) return;
    hasLoaded = true;
    service.getPending().then((result) => {
      if (result.isSuccess) setPendingOperations(result.value!);
    });
  }, [setPendingOperations]);

  const approve = useCallback(
    async (id: string): Promise<boolean> => {
      setPendingId(id);
      const result = await service.approve(id);
      setPendingId(null);
      if (result.isSuccess) {
        removePendingOperation(id);
        addNotification('success', 'Operación aprobada y ejecutada.');
        return true;
      }
      addNotification('error', result.errorMessage ?? 'No se pudo aprobar la operación.');
      return false;
    },
    [removePendingOperation, addNotification],
  );

  const reject = useCallback(
    async (id: string): Promise<boolean> => {
      setPendingId(id);
      const result = await service.reject(id);
      setPendingId(null);
      if (result.isSuccess) {
        removePendingOperation(id);
        addNotification('info', 'Operación rechazada.');
        return true;
      }
      addNotification('error', result.errorMessage ?? 'No se pudo rechazar la operación.');
      return false;
    },
    [removePendingOperation, addNotification],
  );

  /** Drops an operation locally once its 15-minute window lapses (BR-18). */
  const expire = useCallback(
    (id: string) => {
      removePendingOperation(id);
      addNotification('warning', 'Una operación pendiente expiró y fue descartada.');
    },
    [removePendingOperation, addNotification],
  );

  return { pendingOperations, approve, reject, expire, pendingId };
}
