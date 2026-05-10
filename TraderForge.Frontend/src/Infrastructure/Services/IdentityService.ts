import type { RegisterTraderCommand } from '../../Application/DTOs/Commands/RegisterTraderCommand';
import type { LoginTraderQuery } from '../../Application/DTOs/Queries/LoginTraderQuery';
import { Result } from '../../Application/Common/Result';
import { httpClient } from '../Http/httpClient';

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
}

function extractErrorMessage(error: unknown, fallback: string): string {
  const e = error as { response?: { data?: { error?: string } }; code?: string; message?: string };
  if (e?.response?.data?.error) return e.response.data.error;
  if (e?.code === 'ERR_NETWORK' || !e?.response) return 'Cannot reach the server. Make sure the backend is running on port 5116.';
  return fallback;
}
