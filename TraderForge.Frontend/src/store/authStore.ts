import { create } from 'zustand';
import type { Trader } from '@models/Trader';
import { TokenRepository } from '@utils/TokenRepository';
import { getRoleFromToken, type UserRole } from '@utils/jwt';

interface AuthState {
  token: string | null;
  trader: Trader | null;
  isAuthenticated: boolean;
  role: UserRole | null;
  setToken: (token: string) => void;
  setTrader: (trader: Trader) => void;
  logout: () => void;
}

const initialToken = TokenRepository.get();

export const useAuthStore = create<AuthState>((set) => ({
  token: initialToken,
  trader: null,
  isAuthenticated: !!initialToken,
  role: getRoleFromToken(initialToken),

  setToken: (token) => {
    TokenRepository.save(token);
    set({ token, isAuthenticated: true, role: getRoleFromToken(token) });
  },

  setTrader: (trader) => set({ trader }),

  logout: () => {
    TokenRepository.clear();
    set({ token: null, trader: null, isAuthenticated: false, role: null });
  },
}));
