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

  /** Permanently deletes the authenticated trader's account. */
  async deleteAccount(): Promise<Result<void>> {
    try {
      await httpClient.delete('/api/traders/me');
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
