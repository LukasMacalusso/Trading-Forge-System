import type { Portfolio, Position, SimulationSnapshot, Transaction } from '@models/Portfolio';
import { Result } from '@utils/Result';
import { httpClient } from './httpClient';

interface BackendPortfolio {
  id: string;
  virtualBalance: number;
  isActive: boolean;
}

/** A closed (frozen) portfolio returned by the history endpoint. */
interface BackendClosedPortfolio {
  id: string;
  virtualBalance: number;
  createdAt: string;
  closedAt: string | null;
}

interface BackendAsset {
  id: string;
  symbol: string;
  quantity: number;
  entryPrice: number;
}

const ASSET_NAMES: Record<string, string> = {
  BTCUSDT: 'Bitcoin',
  ETHUSDT: 'Ethereum',
  SOLUSDT: 'Solana',
  BNBUSDT: 'BNB',
  XRPUSDT: 'XRP',
};

export class PortfolioService {
  async getPortfolio(initialBalance = 10_000): Promise<Result<Portfolio>> {
    try {
      const [portfolioRes, assetsRes] = await Promise.all([
        httpClient.get<BackendPortfolio>('/api/portfolio'),
        httpClient.get<BackendAsset[]>('/api/portfolio/positions'),
      ]);

      const { id, virtualBalance } = portfolioRes.data;
      const backendAssets = assetsRes.data;

      const positions: Position[] = backendAssets.map((a) => {
        const totalValue = +(a.quantity * a.entryPrice).toFixed(2);
        return {
          id: a.id,
          symbol: a.symbol,
          assetName: ASSET_NAMES[a.symbol] ?? a.symbol,
          quantity: a.quantity,
          averageBuyPrice: a.entryPrice,
          currentPrice: a.entryPrice,
          unrealizedPnL: 0,
          unrealizedPnLPercent: 0,
          totalValue,
        };
      });

      const positionValue = positions.reduce((sum, p) => sum + p.totalValue, 0);
      const totalPortfolioValue = +(virtualBalance + positionValue).toFixed(2);
      const totalPnL = +(totalPortfolioValue - initialBalance).toFixed(2);
      const totalPnLPercent = +((totalPnL / initialBalance) * 100).toFixed(2);

      return Result.ok({
        traderId: id,
        virtualBalance,
        totalPortfolioValue,
        totalPnL,
        totalPnLPercent,
        positions,
      });
    } catch (error) {
      return Result.fail(extractErrorMessage(error, 'Failed to load portfolio.'));
    }
  }

  async buyPosition(symbol: string, quantity: number): Promise<Result<void>> {
    try {
      await httpClient.post('/api/portfolio/positions/buy', { symbol, quantity });
      return Result.ok(undefined);
    } catch (error) {
      return Result.fail(extractErrorMessage(error, 'Failed to buy position.'));
    }
  }

  async sellPosition(assetId: string, quantity: number): Promise<Result<void>> {
    try {
      await httpClient.post(`/api/portfolio/positions/${assetId}/sell`, { quantity });
      return Result.ok(undefined);
    } catch (error) {
      return Result.fail(extractErrorMessage(error, 'Failed to sell position.'));
    }
  }

  /**
   * Past (frozen) simulations, newest first. The backend returns the closed
   * portfolios; P&L is computed against `initialBalance` (the trader's plan
   * starting capital), the same baseline used for the live portfolio.
   */
  async getSimulationHistory(initialBalance = 10_000): Promise<Result<SimulationSnapshot[]>> {
    try {
      const { data } = await httpClient.get<BackendClosedPortfolio[]>('/api/portfolio/history');

      const snapshots: SimulationSnapshot[] = data.map((p) => {
        const finalValue = +p.virtualBalance.toFixed(2);
        const totalPnL = +(finalValue - initialBalance).toFixed(2);
        const totalPnLPercent = initialBalance > 0 ? +((totalPnL / initialBalance) * 100).toFixed(2) : 0;
        return {
          id: p.id,
          createdAt: p.closedAt ?? p.createdAt,
          finalBalance: finalValue,
          finalPortfolioValue: finalValue,
          totalPnL,
          totalPnLPercent,
          positionCount: 0,
        };
      });

      return Result.ok(snapshots);
    } catch (error) {
      return Result.fail(extractErrorMessage(error, 'Failed to load simulation history.'));
    }
  }

  /** The portfolio's balance-movement ledger (deposits, buys, sells). */
  async getTransactions(): Promise<Result<Transaction[]>> {
    try {
      const { data } = await httpClient.get<Transaction[]>('/api/portfolio/transactions');
      return Result.ok(data);
    } catch (error) {
      return Result.fail(extractErrorMessage(error, 'Failed to load transactions.'));
    }
  }

  /** Wipes the trader's positions and restores the initial virtual balance. */
  async resetSimulation(): Promise<Result<void>> {
    try {
      await httpClient.post('/api/portfolio/reset');
      return Result.ok(undefined);
    } catch (error) {
      return Result.fail(extractErrorMessage(error, 'Failed to reset simulation.'));
    }
  }
}

function extractErrorMessage(error: unknown, fallback: string): string {
  const e = error as { response?: { status?: number; data?: { error?: string } }; code?: string };
  if (e?.response?.status === 403 || e?.response?.status === 401) return 'UNAUTHORIZED';
  if (e?.response?.data?.error) return e.response.data.error;
  if (e?.code === 'ERR_NETWORK' || !e?.response) return 'Cannot reach the server. Check the backend is running.';
  return fallback;
}
