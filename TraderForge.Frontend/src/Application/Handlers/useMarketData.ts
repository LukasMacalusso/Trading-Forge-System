import { useEffect, useCallback } from 'react';
import { useMarketStore } from '../Store/marketStore';
import { MarketService } from '../../Infrastructure/Services/MarketService';
import type { Asset } from '../../Domain/Entities/Asset';
import type { CandleInterval } from '../../Domain/Interfaces/IMarketService';
import { MARKET_DATA_MAX_STALENESS_MS } from '../Common/constants';

const marketService = new MarketService();

/** Manages market data fetching and simulates real-time price updates (BR-19). */
export function useMarketData() {
  const { assets, selectedAsset, candles, orderBook, isLoading, lastUpdatedAt,
    setAssets, selectAsset, setCandles, setOrderBook, updateAssetPrice, setLoading } = useMarketStore();

  const isStale = lastUpdatedAt
    ? Date.now() - lastUpdatedAt > MARKET_DATA_MAX_STALENESS_MS
    : false;

  useEffect(() => {
    setLoading(true);
    marketService.getAssets().then((result) => {
      if (result.isSuccess) setAssets(result.value!);
      setLoading(false);
    });
  }, []);

  /** Simulate live price updates every 2 seconds. */
  useEffect(() => {
    if (assets.length === 0) return;
    const interval = setInterval(() => {
      const asset = assets[Math.floor(Math.random() * assets.length)];
      const delta = asset.currentPrice * (Math.random() - 0.499) * 0.003;
      const newPrice = parseFloat((asset.currentPrice + delta).toFixed(2));
      updateAssetPrice(asset.symbol, newPrice, delta);
    }, 2000);
    return () => clearInterval(interval);
  }, [assets]);

  const loadCandles = useCallback(async (symbol: string, interval: CandleInterval = '1h') => {
    const result = await marketService.getCandles(symbol, interval);
    if (result.isSuccess) setCandles(result.value!);
  }, []);

  const loadOrderBook = useCallback(async (symbol: string) => {
    const result = await marketService.getOrderBook(symbol);
    if (result.isSuccess) setOrderBook(result.value!);
  }, []);

  const handleSelectAsset = useCallback((asset: Asset) => {
    selectAsset(asset);
    loadCandles(asset.symbol);
    loadOrderBook(asset.symbol);
  }, [loadCandles, loadOrderBook]);

  return { assets, selectedAsset, candles, orderBook, isLoading, isStale, handleSelectAsset, loadCandles };
}
