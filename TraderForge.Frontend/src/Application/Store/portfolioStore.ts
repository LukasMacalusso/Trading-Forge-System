import { create } from 'zustand';
import type { Portfolio, SimulationSnapshot } from '../../Domain/Entities/Portfolio';
import type { Order } from '../../Domain/Entities/Order';

interface PortfolioState {
  portfolio: Portfolio | null;
  orderHistory: Order[];
  simulationHistory: SimulationSnapshot[];
  isLoading: boolean;
  initialBalance: number;
  setPortfolio: (portfolio: Portfolio) => void;
  setOrderHistory: (orders: Order[]) => void;
  setSimulationHistory: (snapshots: SimulationSnapshot[]) => void;
  setLoading: (loading: boolean) => void;
  setInitialBalance: (balance: number) => void;
  updatePositionPrice: (symbol: string, newPrice: number) => void;
}

export const usePortfolioStore = create<PortfolioState>((set) => ({
  portfolio: null,
  orderHistory: [],
  simulationHistory: [],
  isLoading: false,
  initialBalance: 10_000,

  setPortfolio: (portfolio) => set({ portfolio }),
  setOrderHistory: (orderHistory) => set({ orderHistory }),
  setSimulationHistory: (simulationHistory) => set({ simulationHistory }),
  setLoading: (isLoading) => set({ isLoading }),
  setInitialBalance: (initialBalance) => set({ initialBalance }),

  updatePositionPrice: (symbol, newPrice) =>
    set((state) => {
      if (!state.portfolio) return {};
      const hasPosition = state.portfolio.positions.some((p) => p.symbol === symbol);
      if (!hasPosition) return {};

      const positions = state.portfolio.positions.map((p) => {
        if (p.symbol !== symbol) return p;
        const totalValue = +(newPrice * p.quantity).toFixed(2);
        const unrealizedPnL = +((newPrice - p.averageBuyPrice) * p.quantity).toFixed(2);
        const unrealizedPnLPercent = +(((newPrice - p.averageBuyPrice) / p.averageBuyPrice) * 100).toFixed(2);
        return { ...p, currentPrice: newPrice, totalValue, unrealizedPnL, unrealizedPnLPercent };
      });

      const positionValue = positions.reduce((sum, p) => sum + p.totalValue, 0);
      const totalPortfolioValue = +(state.portfolio.virtualBalance + positionValue).toFixed(2);
      const ib = state.initialBalance;
      const totalPnL = +(totalPortfolioValue - ib).toFixed(2);
      const totalPnLPercent = ib > 0 ? +((totalPnL / ib) * 100).toFixed(2) : 0;

      return {
        portfolio: { ...state.portfolio, positions, totalPortfolioValue, totalPnL, totalPnLPercent },
      };
    }),
}));
