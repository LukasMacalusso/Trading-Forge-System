import { create } from 'zustand';
import type { Trader } from '../../Domain/Entities/Trader';
import { TokenRepository } from '../../Infrastructure/Repositories/TokenRepository';

interface AuthState {
  token: string | null;
  trader: Trader | null;
  isAuthenticated: boolean;
  role: 'Trader' | 'SystemAdmin' | null;
  setToken: (token: string) => void;
  setTrader: (trader: Trader) => void;
  logout: () => void;
}

export const useAuthStore = create<AuthState>((set) => ({
  token: TokenRepository.get(),
  trader: null,
  isAuthenticated: !!TokenRepository.get(),
  role: null,

  setToken: (token) => {
    TokenRepository.save(token);
    set({ token, isAuthenticated: true });
  },

  setTrader: (trader) => set({ trader }),

  logout: () => {
    TokenRepository.clear();
    set({ token: null, trader: null, isAuthenticated: false, role: null });
  },
}));
