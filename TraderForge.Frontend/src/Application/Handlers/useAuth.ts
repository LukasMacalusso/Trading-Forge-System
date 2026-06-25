import { useState } from 'react';
import { useAuthStore } from '../Store/authStore';
import type { RegisterTraderCommand } from '../DTOs/Commands/RegisterTraderCommand';
import type { LoginTraderQuery } from '../DTOs/Queries/LoginTraderQuery';

// NOTE: We are now using IdentityService to connect to the backend
import { IdentityService } from '../../Infrastructure/Services/IdentityService';
const identityService = new IdentityService();

export function useAuth() {
  const { setToken, logout, isAuthenticated, trader } = useAuthStore();
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function register(command: RegisterTraderCommand): Promise<boolean> {
    setIsLoading(true);
    setError(null);
    
    const result = await identityService.register(command);
    
    setIsLoading(false);
    if (result.isSuccess) {
      return true;
    } else {
      setError(result.errorMessage ?? 'Registration failed.');
      return false;
    }
  }

  async function login(query: LoginTraderQuery): Promise<boolean> {
    setIsLoading(true);
    setError(null);

    const result = await identityService.login(query);

    if (result.isSuccess && result.value) {
      setToken(result.value);
      setIsLoading(false);
      return true;
    }

    setIsLoading(false);
    setError(result.errorMessage ?? 'Invalid credentials.');
    return false;
  }

  return { register, login, logout, isAuthenticated, trader, isLoading, error };
}
