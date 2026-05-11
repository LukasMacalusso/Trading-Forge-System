import type { Asset, CandlestickBar, OrderBook } from '../../Domain/Entities/Asset';
import { Result } from '../../Application/Common/Result';
import type { CandleInterval } from '../../Domain/Interfaces/IMarketService';

const INTERVAL_SECONDS: Record<CandleInterval, number> = {
  '1m': 60, '5m': 300, '15m': 900, '1h': 3600, '4h': 14400, '1d': 86400,
};
const INTERVAL_COUNT: Record<CandleInterval, number> = {
  '1m': 150, '5m': 130, '15m': 110, '1h': 100, '4h': 90, '1d': 60,
};
const INTERVAL_VOLATILITY: Record<CandleInterval, number> = {
  '1m': 0.002, '5m': 0.004, '15m': 0.006, '1h': 0.015, '4h': 0.022, '1d': 0.035,
};

// Keep the data logic identical but use the real trading-pair symbols/names so the UI
// looks realistic while remaining mock data.
const MOCK_ASSETS: Asset[] = [
  { symbol: 'BTCUSDT', name: 'Bitcoin', currentPrice: 63_200, priceChange24h: 1200, priceChangePercent24h: 1.93, volume24h: 28_000_000_000, marketCap: 1_240_000_000_000 },
  { symbol: 'ETHUSDT', name: 'Ethereum', currentPrice: 4_200, priceChange24h: 85, priceChangePercent24h: 2.07, volume24h: 18_000_000_000, marketCap: 490_000_000_000 },
  { symbol: 'BNBUSDT', name: 'Binance Coin', currentPrice: 620, priceChange24h: -8, priceChangePercent24h: -1.27, volume24h: 1_200_000_000, marketCap: 96_000_000_000 },
  { symbol: 'ADAUSDT', name: 'Cardano', currentPrice: 0.52, priceChange24h: 0.01, priceChangePercent24h: 1.95, volume24h: 300_000_000, marketCap: 18_000_000_000 },
  { symbol: 'MATICUSDT', name: 'Polygon', currentPrice: 0.88, priceChange24h: -0.02, priceChangePercent24h: -0.45, volume24h: 220_000_000, marketCap: 8_200_000_000 },
  { symbol: 'SOLUSDT', name: 'Solana', currentPrice: 28.5, priceChange24h: 0.7, priceChangePercent24h: 2.51, volume24h: 1_100_000_000, marketCap: 12_400_000_000 },
  { symbol: 'DOTUSDT', name: 'Polkadot', currentPrice: 5.10, priceChange24h: -0.12, priceChangePercent24h: -2.30, volume24h: 95_000_000, marketCap: 6_800_000_000 },
];

function generateCandles(basePrice: number, interval: CandleInterval = '1h'): CandlestickBar[] {
  const bars: CandlestickBar[] = [];
  const count = INTERVAL_COUNT[interval];
  const spacing = INTERVAL_SECONDS[interval];
  const volatility = INTERVAL_VOLATILITY[interval];
  let price = basePrice * 0.85;
  const now = Math.floor(Date.now() / 1000);
  for (let i = count; i >= 0; i--) {
    const v = price * volatility;
    const open = price;
    const close = price + (Math.random() - 0.5) * v * 2;
    bars.push({
      time: now - i * spacing,
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

  async getCandles(symbol: string, interval: CandleInterval = '1h'): Promise<Result<CandlestickBar[]>> {
    await this.delay(300);
    const asset = MOCK_ASSETS.find((a) => a.symbol === symbol);
    return asset ? Result.ok(generateCandles(asset.currentPrice, interval)) : Result.fail(`Asset ${symbol} not found.`);
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
