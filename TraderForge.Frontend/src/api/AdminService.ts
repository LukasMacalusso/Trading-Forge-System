import type { AdminUser, RefundRequest } from '@models/Admin';
import { Result } from '@utils/Result';
import { httpClient } from './httpClient';

/**
 * Admin dashboard endpoints (FR-8, FR-10).
 *
 * NOTE (backend pending): the API only exposes admin plan management today
 * (`/api/admin/plans`). User listing/suspension and refund management are not
 * implemented server-side. These conventional routes let the dashboard work
 * end-to-end once the backend ships them; until then list calls resolve to an
 * empty collection (so the UI shows its empty state) and mutations surface a
 * graceful error.
 */
export class AdminService {
  async getUsers(): Promise<Result<AdminUser[]>> {
    try {
      const { data } = await httpClient.get<AdminUser[]>('/api/admin/users');
      return Result.ok(data);
    } catch (error) {
      if (isMissingBackend(error)) return Result.ok([]);
      return Result.fail(extractErrorMessage(error, 'No se pudieron cargar los usuarios.'));
    }
  }

  async setUserStatus(id: string, action: 'suspend' | 'reactivate'): Promise<Result<void>> {
    try {
      await httpClient.post(`/api/admin/users/${id}/${action}`);
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
