import { create } from 'zustand';
import type { Asset, CandlestickBar, OrderBook } from '../../Domain/Entities/Asset';

interface MarketState {
  assets: Asset[];
  selectedAsset: Asset | null;
  candles: CandlestickBar[];
  orderBook: OrderBook | null;
  lastUpdatedAt: number | null;
  isLoading: boolean;
  setAssets: (assets: Asset[]) => void;
  selectAsset: (asset: Asset) => void;
  setCandles: (candles: CandlestickBar[]) => void;
  setOrderBook: (orderBook: OrderBook) => void;
  updateAssetPrice: (symbol: string, newPrice: number, change: number) => void;
  setLoading: (loading: boolean) => void;
}

export const useMarketStore = create<MarketState>((set) => ({
  assets: [],
  selectedAsset: null,
  candles: [],
  orderBook: null,
  lastUpdatedAt: null,
  isLoading: false,

  setAssets: (assets) => set({ assets, lastUpdatedAt: Date.now() }),

  selectAsset: (asset) => set({ selectedAsset: asset, candles: [], orderBook: null }),

  setCandles: (candles) => set({ candles }),

  setOrderBook: (orderBook) => set({ orderBook }),

  updateAssetPrice: (symbol, newPrice, change) =>
    set((state) => ({
      assets: state.assets.map((a) =>
        a.symbol === symbol
          ? { ...a, currentPrice: newPrice, priceChange24h: change }
          : a
      ),
      selectedAsset:
        state.selectedAsset?.symbol === symbol
          ? { ...state.selectedAsset, currentPrice: newPrice, priceChange24h: change }
          : state.selectedAsset,
      lastUpdatedAt: Date.now(),
    })),

  setLoading: (isLoading) => set({ isLoading }),
}));
