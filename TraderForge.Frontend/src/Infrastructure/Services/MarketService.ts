import type { Asset, CandlestickBar, OrderBook } from '../../Domain/Entities/Asset';
import { Result } from '../../Application/Common/Result';
import type { CandleInterval } from '../../Domain/Interfaces/IMarketService';

const MOCK_ASSETS: Asset[] = [
  { symbol: 'AAPL', name: 'Apple Inc.', currentPrice: 189.84, priceChange24h: 2.34, priceChangePercent24h: 1.25, volume24h: 54_200_000, marketCap: 2_910_000_000_000 },
  { symbol: 'TSLA', name: 'Tesla Inc.', currentPrice: 248.50, priceChange24h: -5.20, priceChangePercent24h: -2.05, volume24h: 78_000_000, marketCap: 790_000_000_000 },
  { symbol: 'MSFT', name: 'Microsoft Corp.', currentPrice: 415.20, priceChange24h: 3.10, priceChangePercent24h: 0.75, volume24h: 22_000_000, marketCap: 3_090_000_000_000 },
  { symbol: 'GOOGL', name: 'Alphabet Inc.', currentPrice: 175.60, priceChange24h: -1.40, priceChangePercent24h: -0.79, volume24h: 18_000_000, marketCap: 2_180_000_000_000 },
  { symbol: 'AMZN', name: 'Amazon.com Inc.', currentPrice: 192.30, priceChange24h: 4.50, priceChangePercent24h: 2.40, volume24h: 31_000_000, marketCap: 2_020_000_000_000 },
  { symbol: 'NVDA', name: 'NVIDIA Corp.', currentPrice: 875.40, priceChange24h: 21.30, priceChangePercent24h: 2.50, volume24h: 41_000_000, marketCap: 2_160_000_000_000 },
  { symbol: 'META', name: 'Meta Platforms Inc.', currentPrice: 521.00, priceChange24h: -3.80, priceChangePercent24h: -0.72, volume24h: 14_000_000, marketCap: 1_320_000_000_000 },
  { symbol: 'BTC', name: 'Bitcoin', currentPrice: 63_200, priceChange24h: 1200, priceChangePercent24h: 1.93, volume24h: 28_000_000_000, marketCap: 1_240_000_000_000 },
];

function generateCandles(basePrice: number, count = 100): CandlestickBar[] {
  const bars: CandlestickBar[] = [];
  let price = basePrice * 0.85;
  const now = Math.floor(Date.now() / 1000);
  for (let i = count; i >= 0; i--) {
    const v = price * 0.015;
    const open = price;
    const close = price + (Math.random() - 0.5) * v * 2;
    bars.push({
      time: now - i * 3600,
      open: +open.toFixed(2),
      high: +(Math.max(open, close) + Math.random() * v).toFixed(2),
      low: +(Math.min(open, close) - Math.random() * v).toFixed(2),
      close: +close.toFixed(2),
      volume: Math.floor(Math.random() * 5_000_000 + 500_000),
    });
    price = close;
  }
  return bars;
}

function generateOrderBook(basePrice: number): Omit<OrderBook, 'symbol'> {
  const side = (dir: 1 | -1) =>
    Array.from({ length: 12 }, (_, i) => {
      const price = +(basePrice + dir * (i + 1) * basePrice * 0.0005).toFixed(2);
      const quantity = +(Math.random() * 500 + 50).toFixed(2);
      return { price, quantity, total: +(price * quantity).toFixed(2) };
    });
  return { bids: side(-1), asks: side(1), timestamp: Date.now() };
}

/** Mock implementation — replace internals with real API calls when backend market endpoints are ready. */
export class MarketService {
  private delay = (ms: number) => new Promise((r) => setTimeout(r, ms));

  async getAssets(): Promise<Result<Asset[]>> {
    await this.delay(300);
    return Result.ok(MOCK_ASSETS);
  }

  async getAssetBySymbol(symbol: string): Promise<Result<Asset>> {
    await this.delay(150);
    const asset = MOCK_ASSETS.find((a) => a.symbol === symbol);
    return asset ? Result.ok(asset) : Result.fail(`Asset ${symbol} not found.`);
  }

  async getCandles(symbol: string, _interval: CandleInterval): Promise<Result<CandlestickBar[]>> {
    await this.delay(400);
    const asset = MOCK_ASSETS.find((a) => a.symbol === symbol);
    return asset ? Result.ok(generateCandles(asset.currentPrice)) : Result.fail(`Asset ${symbol} not found.`);
  }

  async getOrderBook(symbol: string): Promise<Result<OrderBook>> {
    await this.delay(200);
    const asset = MOCK_ASSETS.find((a) => a.symbol === symbol);
    if (!asset) return Result.fail(`Asset ${symbol} not found.`);
    return Result.ok({ ...generateOrderBook(asset.currentPrice), symbol });
  }

  async searchAssets(query: string): Promise<Result<Asset[]>> {
    await this.delay(100);
    const q = query.toLowerCase();
    return Result.ok(MOCK_ASSETS.filter((a) => a.symbol.toLowerCase().includes(q) || a.name.toLowerCase().includes(q)));
  }
}
