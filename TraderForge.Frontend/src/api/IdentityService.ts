import type { RegisterTraderCommand } from '@models/RegisterTraderCommand';
import type { LoginTraderQuery } from '@models/LoginTraderQuery';
import { Result } from '@utils/Result';
import { httpClient } from './httpClient';

/** Real implementation — calls the live backend identity endpoints. */
export class IdentityService {
  async register(command: RegisterTraderCommand): Promise<Result<void>> {
    try {
      await httpClient.post('/api/identity/register', command);
      return Result.ok(undefined);
    } catch (error: unknown) {
      return Result.fail(extractErrorMessage(error, 'Registration failed.'));
    }
  }

  async login(query: LoginTraderQuery): Promise<Result<string>> {
    try {
      const { data } = await httpClient.post<{ token: string }>('/api/identity/login', query);
      return Result.ok(data.token);
    } catch (error: unknown) {
      return Result.fail(extractErrorMessage(error, 'Invalid credentials.'));
    }
  }

  /**
   * Permanently deletes the authenticated trader's account.
   *
   * NOTE (backend pending): the API does not yet expose a self-service delete
   * endpoint — only an admin-scoped `DELETE /api/administrator/{id}` exists.
   * This targets the conventional `DELETE /api/identity` route so the feature
   * works end-to-end the moment the backend adds it. Until then it resolves
   * with an error, which the UI surfaces gracefully.
   */
  async deleteAccount(): Promise<Result<void>> {
    try {
      await httpClient.delete('/api/identity');
      return Result.ok(undefined);
    } catch (error: unknown) {
      return Result.fail(extractErrorMessage(error, 'No se pudo eliminar la cuenta.'));
    }
  }
}

function extractErrorMessage(error: unknown, fallback: string): string {
  const e = error as { response?: { data?: { error?: string } }; code?: string; message?: string };
  if (e?.response?.data?.error) return e.response.data.error;
  if (e?.code === 'ERR_NETWORK' || !e?.response) return 'No se puede conectar al servidor. Asegúrate de que el backend esté corriendo.';
  return fallback;
}
