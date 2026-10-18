import type { Asset, CandlestickBar, OrderBook } from '@models/Asset';
import { Result } from '@utils/Result';
import { httpClient } from './httpClient';
import type { CandleInterval } from '@models/IMarketService';

interface PricesResponse {
  [symbol: string]: number;
}

const SUPPORTED_SYMBOLS = ['BTCUSDT', 'ETHUSDT', 'SOLUSDT', 'BNBUSDT', 'XRPUSDT'];

const ASSET_NAMES: Record<string, string> = {
  BTCUSDT: 'Bitcoin',
  ETHUSDT: 'Ethereum',
  SOLUSDT: 'Solana',
  BNBUSDT: 'BNB',
  XRPUSDT: 'XRP',
};

// =========================================================
// MOCK DATA GENERATORS (FOR SIMULATION ONLY)
// TODO: Remove these once C# Backend implements GetCandles 
// and GetOrderBook endpoints
// =========================================================
const secureRandom = () => crypto.getRandomValues(new Uint32Array(1))[0] / 4294967295;


function generateOrderBook(basePrice: number): Omit<OrderBook, 'symbol'> {
  const side = (dir: 1 | -1) =>
    Array.from({ length: 12 }, (_, i) => {
      const price = +(basePrice + dir * (i + 1) * basePrice * 0.0005).toFixed(2);
      const quantity = +(secureRandom() * 500 + 50).toFixed(2); 
      return { price, quantity, total: +(price * quantity).toFixed(2) };
    });
  return { bids: side(-1), asks: side(1), timestamp: Date.now() };
}

function extractErrorMessage(error: unknown, fallback: string): string {
  const e = error as any;
  if (e?.response?.data?.error) return e.response.data.error;
  if (e?.code === 'ERR_NETWORK') return 'Cannot reach the backend server.';
  return fallback;
}

// =========================================================
// REAL API SERVICE
// =========================================================
let cachedAssets: Asset[] | null = null;

interface BackendCandle {
  openTime: number;
  open: number;
  high: number;
  low: number;
  close: number;
  volume: number;
  closeTime: number;
}

export class MarketService {
  async getAssets(): Promise<Result<Asset[]>> {
    try {
      // Calls real C# Backend endpoint: /api/prices
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
      return Result.fail(extractErrorMessage(error, 'Failed to load live assets.'));
    }
  }

  async getAssetBySymbol(symbol: string): Promise<Result<Asset>> {
    try {
      const { data } = await httpClient.post<PricesResponse>('/api/prices', {
        symbols: [symbol],
      });

      if (data[symbol] == null) return Result.fail(`Asset ${symbol} not found.`);

      return Result.ok({
        symbol,
        name: ASSET_NAMES[symbol] ?? symbol,
        currentPrice: data[symbol],
        priceChange24h: 0,
        priceChangePercent24h: 0,
        volume24h: 0,
        marketCap: 0,
      });
    } catch (error) {
      return Result.fail(extractErrorMessage(error, `Asset ${symbol} not found.`));
    }
  }

  async getCandles(symbol: string, interval: CandleInterval = '1h'): Promise<Result<CandlestickBar[]>> {
    try {
      const { data } = await httpClient.get<BackendCandle[]>('/api/prices/historical', {
        params: { symbol, interval, limit: 500 }
      });
      
      const bars: CandlestickBar[] = data.map(c => ({
        time: Math.floor(c.openTime / 1000),
        open: c.open,
        high: c.high,
        low: c.low,
        close: c.close,
        volume: c.volume,
      }));
      
      return Result.ok(bars);
    } catch (error) {
      return Result.fail(extractErrorMessage(error, 'Could not load historical candles from backend.'));
    }
  }

  // TODO: Point to real C# endpoint
  async getOrderBook(symbol: string): Promise<Result<OrderBook>> {
    try {
      const { data } = await httpClient.post<PricesResponse>('/api/prices', { symbols: [symbol] });
      return Result.ok({ ...generateOrderBook(data[symbol] ?? 100), symbol });
    } catch {
      return Result.fail('Could not load order book');
    }
  }

  async searchAssets(query: string): Promise<Result<Asset[]>> {
    if (!cachedAssets) await this.getAssets();
    const q = query.toLowerCase();
    return Result.ok(
      (cachedAssets ?? []).filter(
        (a) => a.symbol.toLowerCase().includes(q) || a.name.toLowerCase().includes(q)
      )
    );
  }

  getCachedAssets(): Asset[] {
    return cachedAssets ?? [];
  }

  async refreshPrices(): Promise<void> {
    await this.getAssets();
  }
}
