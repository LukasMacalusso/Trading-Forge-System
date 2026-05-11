import { useEffect, useCallback } from 'react';
import { useMarketStore } from '../Store/marketStore';
import { usePortfolioStore } from '../Store/portfolioStore';
import { MarketService } from '../../Infrastructure/Services/MarketService';
import type { Asset } from '../../Domain/Entities/Asset';
import type { CandleInterval } from '../../Domain/Interfaces/IMarketService';
import { MARKET_DATA_MAX_STALENESS_MS } from '../Common/constants';

const marketService = new MarketService();

/** Manages market data fetching and simulates real-time price updates (BR-19). */
export function useMarketData() {
  const {
    assets, watchlist, selectedAsset, candles, orderBook, isLoading, lastUpdatedAt,
    setAssets, selectAsset, setCandles, setOrderBook, updateAssetPrice, setLoading,
    addToWatchlist, removeFromWatchlist,
  } = useMarketStore();

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

  /** Simulate live price updates every 2 seconds and propagate to portfolio for live P&L. */
  useEffect(() => {
    if (assets.length === 0) return;
    const id = setInterval(() => {
      const asset = assets[Math.floor(Math.random() * assets.length)];
      const delta = asset.currentPrice * (Math.random() - 0.499) * 0.003;
      const newPrice = parseFloat((asset.currentPrice + delta).toFixed(2));
      updateAssetPrice(asset.symbol, newPrice, delta);
      usePortfolioStore.getState().updatePositionPrice(asset.symbol, newPrice);
    }, 2000);
    return () => clearInterval(id);
  }, [assets]);

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
