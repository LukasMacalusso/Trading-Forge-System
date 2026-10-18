import { useCallback, useEffect, useState } from 'react';
import type { AdminUser, RefundRequest } from '@models/Admin';
import { AdminService } from '@api/AdminService';
import { useNotificationStore } from '@store/notificationStore';

const service = new AdminService();

export function useAdmin() {
  const addNotification = useNotificationStore((s) => s.addNotification);
  const [users, setUsers] = useState<AdminUser[]>([]);
  const [refunds, setRefunds] = useState<RefundRequest[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [busyId, setBusyId] = useState<string | null>(null);

  useEffect(() => {
    let active = true;
    Promise.all([service.getUsers(), service.getRefundRequests()]).then(([u, r]) => {
      if (!active) return;
      if (u.isSuccess) setUsers(u.value!);
      else addNotification('error', u.errorMessage ?? 'No se pudieron cargar los usuarios.');
      if (r.isSuccess) setRefunds(r.value!);
      setIsLoading(false);
    });
    return () => {
      active = false;
    };
  }, [addNotification]);

  const toggleUserStatus = useCallback(
    async (user: AdminUser) => {
      const action = user.status === 'Active' ? 'suspend' : 'reactivate';
      setBusyId(user.id);
      const result = await service.setUserStatus(user.id, action);
      setBusyId(null);
      if (result.isSuccess) {
        setUsers((prev) =>
          prev.map((u) =>
            u.id === user.id ? { ...u, status: action === 'suspend' ? 'Suspended' : 'Active' } : u,
          ),
        );
        addNotification('success', action === 'suspend' ? 'Cuenta suspendida.' : 'Cuenta reactivada.');
      } else {
        addNotification('error', result.errorMessage ?? 'No se pudo actualizar la cuenta.');
      }
    },
    [addNotification],
  );

  const resolveRefund = useCallback(
    async (refund: RefundRequest, action: 'approve' | 'reject') => {
      setBusyId(refund.id);
      const result = await service.resolveRefund(refund.id, action);
      setBusyId(null);
      if (result.isSuccess) {
        setRefunds((prev) =>
          prev.map((r) =>
            r.id === refund.id ? { ...r, status: action === 'approve' ? 'Approved' : 'Rejected' } : r,
          ),
        );
        addNotification('success', action === 'approve' ? 'Reembolso aprobado.' : 'Reembolso rechazado.');
      } else {
        addNotification('error', result.errorMessage ?? 'No se pudo procesar la solicitud.');
      }
    },
    [addNotification],
  );

  return { users, refunds, isLoading, busyId, toggleUserStatus, resolveRefund };
}
