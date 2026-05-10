import { create } from 'zustand';
import type { Portfolio, SimulationSnapshot } from '../../Domain/Entities/Portfolio';
import type { Order } from '../../Domain/Entities/Order';

interface PortfolioState {
  portfolio: Portfolio | null;
  orderHistory: Order[];
  simulationHistory: SimulationSnapshot[];
  isLoading: boolean;
  setPortfolio: (portfolio: Portfolio) => void;
  setOrderHistory: (orders: Order[]) => void;
  setSimulationHistory: (snapshots: SimulationSnapshot[]) => void;
  setLoading: (loading: boolean) => void;
}

export const usePortfolioStore = create<PortfolioState>((set) => ({
  portfolio: null,
  orderHistory: [],
  simulationHistory: [],
  isLoading: false,

  setPortfolio: (portfolio) => set({ portfolio }),
  setOrderHistory: (orderHistory) => set({ orderHistory }),
  setSimulationHistory: (simulationHistory) => set({ simulationHistory }),
  setLoading: (isLoading) => set({ isLoading }),
}));
