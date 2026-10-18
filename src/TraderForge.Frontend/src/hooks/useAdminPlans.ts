import { useCallback, useEffect, useState } from 'react';
import type { AdminPlan, PlanFormData } from '@models/Admin';
import { AdminService } from '@api/AdminService';
import { useNotificationStore } from '@store/notificationStore';

const service = new AdminService();

export function useAdminPlans() {
  const addNotification = useNotificationStore((s) => s.addNotification);
  const [plans, setPlans] = useState<AdminPlan[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [busyId, setBusyId] = useState<string | null>(null);

  const refresh = useCallback(async () => {
    const result = await service.getPlans();
    if (result.isSuccess) setPlans(result.value!);
    else if (result.errorMessage !== 'UNAUTHORIZED') {
      addNotification('error', result.errorMessage ?? 'No se pudieron cargar los planes.');
    }
    setIsLoading(false);
  }, [addNotification]);

  useEffect(() => {
    refresh();
  }, [refresh]);

  const createPlan = useCallback(
    async (data: PlanFormData): Promise<boolean> => {
      setBusyId('new');
      const result = await service.createPlan(data);
      setBusyId(null);
      if (result.isSuccess) {
        addNotification('success', 'Plan creado.');
        await refresh();
        return true;
      }
      addNotification('error', result.errorMessage ?? 'No se pudo crear el plan.');
      return false;
    },
    [addNotification, refresh],
  );

  const updatePlan = useCallback(
    async (id: string, data: PlanFormData): Promise<boolean> => {
      setBusyId(id);
      const result = await service.updatePlan(id, data);
      setBusyId(null);
      if (result.isSuccess) {
        addNotification('success', 'Plan actualizado.');
        await refresh();
        return true;
      }
      addNotification('error', result.errorMessage ?? 'No se pudo actualizar el plan.');
      return false;
    },
    [addNotification, refresh],
  );

  const deletePlan = useCallback(
    async (id: string): Promise<boolean> => {
      setBusyId(id);
      const result = await service.deletePlan(id);
      setBusyId(null);
      if (result.isSuccess) {
        addNotification('success', 'Plan eliminado.');
        await refresh();
        return true;
      }
      addNotification('error', result.errorMessage ?? 'No se pudo eliminar el plan.');
      return false;
    },
    [addNotification, refresh],
  );

  return { plans, isLoading, busyId, createPlan, updatePlan, deletePlan };
}
