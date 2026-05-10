import type { Portfolio, SimulationSnapshot } from '../../Domain/Entities/Portfolio';
import { Result } from '../../Application/Common/Result';

const MOCK_PORTFOLIO: Portfolio = {
  traderId: 'mock-trader-id',
  virtualBalance: 6_843.17,
  totalPortfolioValue: 13_156.83,
  totalPnL: 3_156.83,
  totalPnLPercent: 31.57,
  positions: [
    { symbol: 'AAPL', assetName: 'Apple Inc.', quantity: 5, averageBuyPrice: 182.50, currentPrice: 189.84, unrealizedPnL: 36.70, unrealizedPnLPercent: 4.02, totalValue: 949.20 },
    { symbol: 'TSLA', assetName: 'Tesla Inc.', quantity: 5, averageBuyPrice: 245.00, currentPrice: 248.50, unrealizedPnL: 17.50, unrealizedPnLPercent: 1.43, totalValue: 1242.50 },
    { symbol: 'NVDA', assetName: 'NVIDIA Corp.', quantity: 10, averageBuyPrice: 720.00, currentPrice: 875.40, unrealizedPnL: 1554.00, unrealizedPnLPercent: 21.58, totalValue: 8754.00 },
    { symbol: 'MSFT', assetName: 'Microsoft Corp.', quantity: 5, averageBuyPrice: 400.00, currentPrice: 415.20, unrealizedPnL: 76.00, unrealizedPnLPercent: 3.80, totalValue: 2076.00 },
  ],
};

const MOCK_SNAPSHOTS: SimulationSnapshot[] = [
  { id: 'snap-1', createdAt: '2026-03-01T12:00:00Z', finalBalance: 8200, finalPortfolioValue: 12000, totalPnL: 2000, totalPnLPercent: 20, positionCount: 3 },
  { id: 'snap-2', createdAt: '2026-04-01T12:00:00Z', finalBalance: 3500, finalPortfolioValue: 9800, totalPnL: -200, totalPnLPercent: -2, positionCount: 5 },
];

/** Mock implementation — replace with real API calls when backend portfolio endpoints are ready. */
export class PortfolioService {
  private delay = (ms: number) => new Promise((r) => setTimeout(r, ms));

  async getPortfolio(_traderId: string): Promise<Result<Portfolio>> {
    await this.delay(300);
    return Result.ok({ ...MOCK_PORTFOLIO, positions: [...MOCK_PORTFOLIO.positions] });
  }

  async getSimulationHistory(_traderId: string): Promise<Result<SimulationSnapshot[]>> {
    await this.delay(200);
    return Result.ok(MOCK_SNAPSHOTS);
  }

  async resetSimulation(_traderId: string): Promise<Result<void>> {
    await this.delay(500);
    MOCK_PORTFOLIO.virtualBalance = 10_000;
    MOCK_PORTFOLIO.positions = [];
    MOCK_PORTFOLIO.totalPortfolioValue = 10_000;
    MOCK_PORTFOLIO.totalPnL = 0;
    MOCK_PORTFOLIO.totalPnLPercent = 0;
    return Result.ok(undefined);
  }
}
