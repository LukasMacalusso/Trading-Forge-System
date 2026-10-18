import type { PendingOperation } from '@models/BotFlow';
import { Result } from '@utils/Result';
import { httpClient } from './httpClient';

/** Raw shape expected from the backend pending-operations endpoint. */
interface BackendPendingOperation {
  id: string;
  flowId: string;
  flowName: string;
  symbol: string;
  action: string;
  quantity: number;
  currentPrice: number;
  conditionMetAt: string;
  expiresAt: string;
}

const BASE = '/api/portfolio/pending-operations';

/**
 * Talks to the manual-approval (FR-14) endpoints.
 *
 * NOTE (backend pending): the API does not yet expose pending-operation
 * routes — this depends on "4.3 Notification Bots & Manual Approvals", which
 * is server-side and not implemented. The conventional routes below let the
 * feature work end-to-end the moment the backend ships them. Until then
 * `getPending` resolves to an empty list (so the UI shows its empty state
 * rather than an error) and approve/reject surface a graceful failure.
 */
export class PendingOperationsService {
  async getPending(): Promise<Result<PendingOperation[]>> {
    try {
      const { data } = await httpClient.get<BackendPendingOperation[]>(BASE);
      return Result.ok(data.map(mapPendingOperation));
    } catch (error) {
      // Treat an unreachable/absent endpoint as "nothing pending" — not an error.
      if (isMissingBackend(error)) return Result.ok([]);
      return Result.fail(extractErrorMessage(error, 'Failed to load pending operations.'));
    }
  }

  async approve(id: string): Promise<Result<void>> {
    try {
      await httpClient.post(`${BASE}/${id}/approve`);
      return Result.ok(undefined);
    } catch (error) {
      return Result.fail(extractErrorMessage(error, 'No se pudo aprobar la operación.'));
    }
  }

  async reject(id: string): Promise<Result<void>> {
    try {
      await httpClient.post(`${BASE}/${id}/reject`);
      return Result.ok(undefined);
    } catch (error) {
      return Result.fail(extractErrorMessage(error, 'No se pudo rechazar la operación.'));
    }
  }
}

function mapPendingOperation(o: BackendPendingOperation): PendingOperation {
  return {
    id: o.id,
    flowId: o.flowId,
    flowName: o.flowName,
    symbol: o.symbol,
    action: o.action === 'Sell' ? 'Sell' : 'Buy',
    quantity: o.quantity,
    currentPrice: o.currentPrice,
    conditionMetAt: o.conditionMetAt,
    expiresAt: o.expiresAt,
  };
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
