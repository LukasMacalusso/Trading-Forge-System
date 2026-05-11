import type { Asset, CandlestickBar, OrderBook } from '../../Domain/Entities/Asset';
import { Result } from '../../Application/Common/Result';
import { httpClient } from '../Http/httpClient';
import type { CandleInterval } from '../../Domain/Interfaces/IMarketService';

interface PricesResponse {
  [symbol: string]: number;
}

const ASSET_NAMES: Record<string, string> = {
  BTCUSDT: 'Bitcoin',
  ETHUSDT: 'Ethereum',
  SOLUSDT: 'Solana',
  BNBUSDT: 'BNB',
  XRPUSDT: 'XRP',
};

const INTERVAL_SECONDS: Record<CandleInterval, number> = {
  '1m': 60, '5m': 300, '15m': 900, '1h': 3600, '4h': 14400, '1d': 86400,
};
const INTERVAL_COUNT: Record<CandleInterval, number> = {
  '1m': 150, '5m': 130, '15m': 110, '1h': 100, '4h': 90, '1d': 60,
};
const INTERVAL_VOLATILITY: Record<CandleInterval, number> = {
  '1m': 0.002, '5m': 0.004, '15m': 0.006, '1h': 0.015, '4h': 0.022, '1d': 0.035,
};

const SUPPORTED_SYMBOLS = Object.keys(ASSET_NAMES);

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

function extractErrorMessage(error: unknown, fallback: string): string {
  const e = error as { response?: { data?: { error?: string } }; code?: string };
  if (e?.response?.data?.error) return e.response.data.error;
  if (e?.code === 'ERR_NETWORK' || !e?.response) return 'Cannot reach the server. Check the backend is running.';
  return fallback;
}

let cachedAssets: Asset[] | null = null;

export class MarketService {
  async getAssets(): Promise<Result<Asset[]>> {
    try {
      const { data } = await httpClient.post<PricesResponse>('/api/prices', {
        symbols: SUPPORTED_SYMBOLS,
      });

      const assets: Asset[] = SUPPORTED_SYMBOLS.map((symbol) => ({
        symbol,
        name: ASSET_NAMES[symbol] ?? symbol,
        currentPrice: data[symbol] ?? 0,
        priceChange24h: 0,
        priceChangePercent24h: 0,
        volume24h: 0,
        marketCap: 0,
      }));

      cachedAssets = assets;
      return Result.ok(assets);
    } catch (error) {
      if (cachedAssets) return Result.ok(cachedAssets);
      return Result.fail(extractErrorMessage(error, 'Failed to load assets.'));
    }
  }

  async getAssetBySymbol(symbol: string): Promise<Result<Asset>> {
    try {
      const { data } = await httpClient.post<PricesResponse>('/api/prices', {
        symbols: [symbol],
      });

      const price = data[symbol];
      if (price == null) return Result.fail(`Asset ${symbol} not found.`);

      return Result.ok({
        symbol,
        name: ASSET_NAMES[symbol] ?? symbol,
        currentPrice: price,
        priceChange24h: 0,
        priceChangePercent24h: 0,
        volume24h: 0,
        marketCap: 0,
      });
    } catch (error) {
      if (cachedAssets) {
        const asset = cachedAssets.find((a) => a.symbol === symbol);
        if (asset) return Result.ok(asset);
      }
      return Result.fail(extractErrorMessage(error, `Asset ${symbol} not found.`));
    }
  }

  async getCandles(symbol: string, interval: CandleInterval = '1h'): Promise<Result<CandlestickBar[]>> {
    try {
      const { data } = await httpClient.post<PricesResponse>('/api/prices', {
        symbols: [symbol],
      });
      const price = data[symbol] ?? 100;
      return Result.ok(generateCandles(price, interval));
    } catch {
      return Result.ok(generateCandles(100, interval));
    }
  }

  async getOrderBook(symbol: string): Promise<Result<OrderBook>> {
    try {
      const { data } = await httpClient.post<PricesResponse>('/api/prices', {
        symbols: [symbol],
      });
      const price = data[symbol] ?? 100;
      return Result.ok({ ...generateOrderBook(price), symbol });
    } catch {
      return Result.ok({ ...generateOrderBook(100), symbol });
    }
  }

  async searchAssets(query: string): Promise<Result<Asset[]>> {
    try {
      if (!cachedAssets) await this.getAssets();
      const q = query.toLowerCase();
      return Result.ok(
        (cachedAssets ?? []).filter(
          (a) => a.symbol.toLowerCase().includes(q) || a.name.toLowerCase().includes(q)
        )
      );
    } catch {
      return Result.ok([]);
    }
  }

  async refreshPrices(): Promise<void> {
    try {
      const { data } = await httpClient.post<PricesResponse>('/api/prices', {
        symbols: SUPPORTED_SYMBOLS,
      });

      if (cachedAssets) {
        cachedAssets = cachedAssets.map((asset) => ({
          ...asset,
          currentPrice: data[asset.symbol] ?? asset.currentPrice,
        }));
      }
    } catch {
      // silent fail — keep stale prices
    }
  }

  getCachedAssets(): Asset[] {
    return cachedAssets ?? [];
  }
}
