import type { Portfolio, SimulationSnapshot, Position } from '../../Domain/Entities/Portfolio';
import { Result } from '../../Application/Common/Result';
import { httpClient } from '../Http/httpClient';

interface BackendPortfolio {
  id: string;
  virtualBalance: number;
  isActive: boolean;
}

interface BackendAsset {
  id: string;
  symbol: string;
  quantity: number;
  entryPrice: number;
}

const ASSET_NAMES: Record<string, string> = {
  AAPL: 'Apple Inc.', TSLA: 'Tesla Inc.', MSFT: 'Microsoft Corp.',
  GOOGL: 'Alphabet Inc.', AMZN: 'Amazon.com Inc.', NVDA: 'NVIDIA Corp.',
  META: 'Meta Platforms Inc.', BTC: 'Bitcoin',
};

export class PortfolioService {
  async getPortfolio(initialBalance = 10_000): Promise<Result<Portfolio>> {
    try {
      const [portfolioRes, assetsRes] = await Promise.all([
        httpClient.get<BackendPortfolio>('/api/portfolio'),
        httpClient.get<BackendAsset[]>('/api/portfolio/assets'),
      ]);

      const { id, virtualBalance } = portfolioRes.data;
      const backendAssets = assetsRes.data;

      const positions: Position[] = backendAssets.map((a) => ({
        id: a.id,
        symbol: a.symbol,
        assetName: ASSET_NAMES[a.symbol] ?? a.symbol,
        quantity: a.quantity,
        averageBuyPrice: a.entryPrice,
        currentPrice: a.entryPrice,
        unrealizedPnL: 0,
        unrealizedPnLPercent: 0,
        totalValue: +(a.quantity * a.entryPrice).toFixed(2),
      }));

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

  async addAsset(symbol: string, quantity: number, entryPrice: number): Promise<Result<void>> {
    try {
      await httpClient.post('/api/portfolio/assets', { symbol, quantity, entryPrice });
      return Result.ok(undefined);
    } catch (error) {
      return Result.fail(extractErrorMessage(error, 'Failed to add asset.'));
    }
  }

  async removeAsset(assetId: string): Promise<Result<void>> {
    try {
      await httpClient.delete(`/api/portfolio/assets/${assetId}`);
      return Result.ok(undefined);
    } catch (error) {
      return Result.fail(extractErrorMessage(error, 'Failed to remove asset.'));
    }
  }

  async getSimulationHistory(): Promise<Result<SimulationSnapshot[]>> {
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
