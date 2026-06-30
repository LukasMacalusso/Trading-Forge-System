import type { AdminUser, RefundRequest } from '@models/Admin';
import type { SubscriptionPlan } from '@models/Trader';
import { Result } from '@utils/Result';
import { httpClient } from './httpClient';

interface BackendTrader {
  id: string;
  email: string;
  isSuspended: boolean;
  suspensionReason: string | null;
  activePlanId: string | null;
}

interface BackendPlan {
  id: string;
  name: string;
}

/**
 * Admin dashboard endpoints (FR-8, FR-10). User listing and suspension map to
 * the backend `/api/admin/traders` routes. Refund management has no server
 * counterpart yet, so those calls resolve to an empty collection and the UI
 * shows its empty state.
 */
export class AdminService {
  async getUsers(): Promise<Result<AdminUser[]>> {
    try {
      const [tradersRes, plansRes] = await Promise.all([
        httpClient.get<BackendTrader[]>('/api/admin/traders'),
        httpClient.get<BackendPlan[]>('/api/subscription/plans'),
      ]);

      const planNameById = new Map(plansRes.data.map((p) => [p.id, p.name]));

      const users: AdminUser[] = tradersRes.data.map((t) => ({
        id: t.id,
        email: t.email,
        userName: t.email.split('@')[0],
        plan: (t.activePlanId ? planNameById.get(t.activePlanId) : undefined) as SubscriptionPlan | undefined,
        status: t.isSuspended ? 'Suspended' : 'Active',
      }));

      return Result.ok(users);
    } catch (error) {
      if (isMissingBackend(error)) return Result.ok([]);
      return Result.fail(extractErrorMessage(error, 'No se pudieron cargar los usuarios.'));
    }
  }

  async setUserStatus(id: string, action: 'suspend' | 'reactivate'): Promise<Result<void>> {
    try {
      if (action === 'suspend') {
        await httpClient.post(`/api/admin/traders/${id}/suspend`, {
          reason: 'Suspendido por el administrador',
        });
      } else {
        await httpClient.post(`/api/admin/traders/${id}/unsuspend`);
      }
      return Result.ok(undefined);
    } catch (error) {
      return Result.fail(extractErrorMessage(error, 'No se pudo actualizar el estado de la cuenta.'));
    }
  }

  async getRefundRequests(): Promise<Result<RefundRequest[]>> {
    try {
      const { data } = await httpClient.get<RefundRequest[]>('/api/admin/refunds');
      return Result.ok(data);
    } catch (error) {
      if (isMissingBackend(error)) return Result.ok([]);
      return Result.fail(extractErrorMessage(error, 'No se pudieron cargar los reembolsos.'));
    }
  }

  async resolveRefund(id: string, action: 'approve' | 'reject'): Promise<Result<void>> {
    try {
      await httpClient.post(`/api/admin/refunds/${id}/${action}`);
      return Result.ok(undefined);
    } catch (error) {
      return Result.fail(extractErrorMessage(error, 'No se pudo procesar la solicitud.'));
    }
  }
}

function isMissingBackend(error: unknown): boolean {
  const e = error as { response?: { status?: number }; code?: string };
  return e?.code === 'ERR_NETWORK' || !e?.response || e.response.status === 404;
}

function extractErrorMessage(error: unknown, fallback: string): string {
  const e = error as { response?: { data?: { error?: string } }; code?: string };
  if (e?.response?.data?.error) return e.response.data.error;
  if (e?.code === 'ERR_NETWORK' || !e?.response) return 'No se puede conectar al servidor.';
  return fallback;
}
