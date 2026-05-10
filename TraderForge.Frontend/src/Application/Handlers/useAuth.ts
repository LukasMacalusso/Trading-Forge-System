import { useState } from 'react';
import { useAuthStore } from '../Store/authStore';
import type { RegisterTraderCommand } from '../DTOs/Commands/RegisterTraderCommand';
import type { LoginTraderQuery } from '../DTOs/Queries/LoginTraderQuery';

// NOTE: We are intentionally NOT using IdentityService right now to avoid backend dependency
// import { IdentityService } from '../../Infrastructure/Services/IdentityService';
// const identityService = new IdentityService();

export function useAuth() {
  const { setToken, logout, isAuthenticated, trader } = useAuthStore();
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function register(command: RegisterTraderCommand): Promise<boolean> {
    setIsLoading(true);
    setError(null);
    
    // Simulate network delay
    await new Promise(resolve => setTimeout(resolve, 800));
    
    // Mock successful registration
    console.log('[MOCK AUTH] Registered:', command.email);
    setIsLoading(false);
    return true;
  }

  async function login(query: LoginTraderQuery): Promise<boolean> {
    setIsLoading(true);
    setError(null);
    
    // Simulate network delay
    await new Promise(resolve => setTimeout(resolve, 800));

    // Mock successful login - just check if it's not empty for the mock
    if (query.email && query.password) {
      console.log('[MOCK AUTH] Logged in:', query.email);
      // Se da la token al store de autenticación para simular que se inicia sesion
      setToken('mock-jwt-token-for-' + query.email);
      setIsLoading(false);
      return true;
    }

    setIsLoading(false);
    setError('Invalid credentials.');
    return false;
  }

  return { register, login, logout, isAuthenticated, trader, isLoading, error };
}
