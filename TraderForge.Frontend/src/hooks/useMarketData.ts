import { useEffect, useCallback, useRef } from 'react';
import { useMarketStore } from '@store/marketStore';
import { usePortfolioStore } from '@store/portfolioStore';
import { MarketService } from '@api/MarketService';
import type { Asset } from '@models/Asset';
import type { CandleInterval } from '@models/IMarketService';
import { MARKET_DATA_MAX_STALENESS_MS } from '@utils/constants';

const marketService = new MarketService();

export function useMarketData() {
  const {
    assets, watchlist, selectedAsset, candles, orderBook, isLoading, lastUpdatedAt,
    setAssets, selectAsset, setCandles, setOrderBook, setLoading,
    addToWatchlist, removeFromWatchlist,
  } = useMarketStore();

  const intervalRef = useRef<ReturnType<typeof setInterval> | null>(null);

  const isStale = lastUpdatedAt
    ? Date.now() - lastUpdatedAt > MARKET_DATA_MAX_STALENESS_MS
    : false;

  const watchedAssets = assets.filter((a) => watchlist.includes(a.symbol));
  const unwatchedAssets = assets.filter((a) => !watchlist.includes(a.symbol));

  useEffect(() => {
    setLoading(true);
    marketService.getAssets().then((result) => {
      if (result.isSuccess) setAssets(result.value!);
      setLoading(false);
    });
  }, []);

  useEffect(() => {
    if (assets.length === 0) return;

    intervalRef.current = setInterval(async () => {
      await marketService.refreshPrices();
      const updated = marketService.getCachedAssets();
      if (updated.length > 0) {
        setAssets(updated);
        updated.forEach((asset) => {
          usePortfolioStore.getState().updatePositionPrice(asset.symbol, asset.currentPrice);
        });
      }
    }, 5000);

    return () => {
      if (intervalRef.current) clearInterval(intervalRef.current);
    };
  }, [assets.length > 0]);

  const loadCandles = useCallback(async (symbol: string, interval: CandleInterval = '1h') => {
    const result = await marketService.getCandles(symbol, interval);
    if (result.isSuccess) setCandles(result.value!);
  }, []);

  const loadOrderBook = useCallback(async (symbol: string) => {
    const result = await marketService.getOrderBook(symbol);
    if (result.isSuccess) setOrderBook(result.value!);
  }, []);

  const handleSelectAsset = useCallback((asset: Asset, interval: CandleInterval = '1h') => {
    selectAsset(asset);
    loadCandles(asset.symbol, interval);
    loadOrderBook(asset.symbol);
  }, [loadCandles, loadOrderBook, selectAsset]);

  return {
    assets,
    watchedAssets,
    unwatchedAssets,
    selectedAsset,
    candles,
    orderBook,
    isLoading,
    isStale,
    handleSelectAsset,
    loadCandles,
    addToWatchlist,
    removeFromWatchlist,
  };
}
