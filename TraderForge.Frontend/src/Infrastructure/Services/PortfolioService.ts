import type { Portfolio, Position } from '../../Domain/Entities/Portfolio';
import { Result } from '../../Application/Common/Result';
import { httpClient } from '../Http/httpClient';

interface BackendPortfolio {
  id: string;
  virtualBalance: number;
  isActive: boolean;
}

interface BackendPosition {
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
      const [portfolioRes, positionsRes] = await Promise.all([
        httpClient.get<BackendPortfolio>('/api/portfolio'),
        httpClient.get<BackendPosition[]>('/api/portfolio/positions'),
      ]);

      const { id, virtualBalance } = portfolioRes.data;
      const backendPositions = positionsRes.data;

      const positions: Position[] = backendPositions.map((p) => {
        const totalValue = +(p.quantity * p.entryPrice).toFixed(2);
        return {
          id: p.id,
          symbol: p.symbol,
          assetName: ASSET_NAMES[p.symbol] ?? p.symbol,
          quantity: p.quantity,
          averageBuyPrice: p.entryPrice,
          currentPrice: p.entryPrice,
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

  async buyPosition(symbol: string, quantity: number, entryPrice: number): Promise<Result<void>> {
    try {
      await httpClient.post('/api/portfolio/positions/buy', { symbol, quantity, entryPrice });
      return Result.ok(undefined);
    } catch (error) {
      return Result.fail(extractErrorMessage(error, 'Failed to buy position.'));
    }
  }

  async sellPosition(positionId: string, quantity: number): Promise<Result<void>> {
    try {
      await httpClient.post(`/api/portfolio/positions/${positionId}/sell`, { quantity });
      return Result.ok(undefined);
    } catch (error) {
      return Result.fail(extractErrorMessage(error, 'Failed to sell position.'));
    }
  }

  async getSimulationHistory(): Promise<Result<[]>> {
    return Result.ok([]);
  }
}

function extractErrorMessage(error: unknown, fallback: string): string {
  const e = error as { response?: { status?: number; data?: { error?: string } }; code?: string };
  if (e?.response?.status === 403 || e?.response?.status === 401) return 'UNAUTHORIZED';
  if (e?.response?.data?.error) return e.response.data.error;
  if (e?.code === 'ERR_NETWORK' || !e?.response) return 'Cannot reach the server. Check the backend is running.';
  return fallback;
}
