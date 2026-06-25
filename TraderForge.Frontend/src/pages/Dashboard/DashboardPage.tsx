import { useEffect, useState } from 'react';
import { AlertTriangle } from 'lucide-react';
import type { CandleInterval } from '@models/IMarketService';

const INTERVALS: { label: string; value: CandleInterval }[] = [
  { label: '1m', value: '1m' },
  { label: '5m', value: '5m' },
  { label: '15m', value: '15m' },
  { label: '1h', value: '1h' },
  { label: '4h', value: '4h' },
  { label: '1D', value: '1d' },
];
import { useMarketData } from '@hooks/useMarketData';
import { usePortfolioStore } from '@store/portfolioStore';
import { usePortfolio } from '@hooks/usePortfolio';
import { PriceTicker } from '@components/Ticker/PriceTicker';
import { CandlestickChart } from '@components/Charts/CandlestickChart';
import { OrderBook } from '@components/OrderBook/OrderBook';
import { ExecutionPanel } from '@components/Orders/ExecutionPanel';
import { Badge } from '@components/UI/Badge';

export function DashboardPage() {
  const [interval, setIntervalValue] = useState<CandleInterval>('1h');

  const {
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
  } = useMarketData();

  function handleIntervalChange(newInterval: CandleInterval) {
    setIntervalValue(newInterval);
    if (selectedAsset) loadCandles(selectedAsset.symbol, newInterval);
  }
  const { portfolio } = usePortfolioStore();
  usePortfolio();

  // Sync portfolio positions to watchlist
  useEffect(() => {
    if (portfolio?.positions) {
      portfolio.positions.forEach((pos) => addToWatchlist(pos.symbol));
    }
  }, [portfolio]);

  useEffect(() => {
    if (watchedAssets.length > 0 && !selectedAsset) {
      handleSelectAsset(watchedAssets[0], interval);
    }
  }, [watchedAssets]);

  const pnlIsUp = (portfolio?.totalPnL ?? 0) >= 0;
  const userEntryPrice = selectedAsset
    ? portfolio?.positions.find((p) => p.symbol === selectedAsset.symbol)?.averageBuyPrice
    : undefined;

  const hasPositions = (portfolio?.positions.length ?? 0) > 0;
  const hasWatchedAssets = watchedAssets.length > 0;

  return (
    <div className="flex flex-col h-full min-h-0">
      {/* Stale data warning */}
      {isStale && (
        <div className="flex items-center gap-2 px-4 py-2 bg-amber-500/10 border-b border-amber-500/20 text-amber-400 text-xs">
          <AlertTriangle size={14} />
          Market data may be delayed. Refresh if prices seem incorrect.
        </div>
      )}

      {/* Price Ticker — solo activos del portfolio */}
      {isLoading ? (
        <div className="h-14 border-b border-neutral-800 flex items-center px-4">
          <span className="text-xs text-neutral-600 animate-pulse">Loading market data...</span>
        </div>
      ) : (
        <PriceTicker
          assets={watchedAssets}
          allAssets={assets}
          selectedSymbol={selectedAsset?.symbol}
          onSelect={(asset) => handleSelectAsset(asset, interval)}
          onAdd={(symbol) => {
            addToWatchlist(symbol);
            const asset = assets.find((a) => a.symbol === symbol);
            if (asset) handleSelectAsset(asset, interval);
          }}
          onRemove={removeFromWatchlist}
        />
      )}

      {/* Main content */}
      <div className="flex flex-1 min-h-0 gap-0">
        {/* Center: Chart */}
        <div className="flex flex-col flex-1 min-w-0 min-h-0 p-3 gap-3">

          {/* Welcome state — sin activos en watchlist */}
          {!isLoading && !hasWatchedAssets && (
            <div className="flex-1 flex flex-col items-center justify-center gap-3 text-center">
              <div className="w-16 h-16 rounded-full bg-neutral-900 border border-neutral-800 flex items-center justify-center">
                <span className="text-2xl">📊</span>
              </div>
              <div>
                <p className="text-neutral-200 font-medium">Parece que no tienes nada</p>
                <p className="text-xs text-neutral-500 mt-1">
                  Añade un activo con el botón{' '}
                  <span className="text-amber-400 font-semibold">+ Añadir</span>{' '}
                  para empezar a operar
                </p>
              </div>
            </div>
          )}

          {/* Con activos: asset header + chart */}
          {hasWatchedAssets && (
            <>
              {/* Asset header */}
              {selectedAsset && (
                <div className="flex items-center gap-4 px-1 flex-wrap">
                  <div>
                    <span className="text-lg font-bold text-neutral-100">{selectedAsset.symbol}</span>
                    <span className="text-sm text-neutral-500 ml-2">{selectedAsset.name}</span>
                  </div>
                  <span className="text-xl font-mono font-bold text-neutral-100">
                    ${selectedAsset.currentPrice.toLocaleString('en-US', { minimumFractionDigits: 2 })}
                  </span>
                  <Badge variant={selectedAsset.priceChange24h >= 0 ? 'up' : 'down'}>
                    {selectedAsset.priceChange24h >= 0 ? '+' : ''}
                    {selectedAsset.priceChangePercent24h.toFixed(2)}%
                  </Badge>
                  {/* Interval selector */}
                  <div className="flex items-center gap-0.5 ml-2">
                    {INTERVALS.map(({ label, value }) => (
                      <button
                        key={value}
                        onClick={() => handleIntervalChange(value)}
                        className={`px-2 py-1 text-[11px] font-mono rounded transition-colors ${
                          interval === value
                            ? 'bg-amber-500/20 text-amber-400 border border-amber-500/30'
                            : 'text-neutral-600 hover:text-neutral-300 hover:bg-neutral-800'
                        }`}
                      >
                        {label}
                      </button>
                    ))}
                  </div>
                  <span className="text-xs text-neutral-600 ml-auto">
                    Vol: {(selectedAsset.volume24h / 1_000_000).toFixed(1)}M
                  </span>
                </div>
              )}

              {/* No asset selected */}
              {!selectedAsset && (
                <div className="flex-1 flex items-center justify-center text-neutral-600 text-sm">
                  Selecciona un activo del ticker
                </div>
              )}

              {/* Candlestick Chart */}
              {selectedAsset && (
                <div className="flex-1 min-h-0 bg-[#0a0a0b] rounded-lg overflow-hidden relative">
                  <div className="absolute inset-0">
                    <CandlestickChart candles={candles} symbol={selectedAsset.symbol} />
                  </div>
                </div>
              )}
            </>
          )}
        </div>

        {/* Right panel: Balance + Order Book + Execution */}
        <div className="w-72 shrink-0 flex flex-col gap-3 p-3 border-l border-neutral-800">
          {/* Balance summary */}
          {portfolio && (
            <div className="bg-neutral-900 rounded-lg border border-neutral-800 p-3 text-xs flex flex-col gap-2 shrink-0">
              <div className="flex justify-between items-center">
                <span className="text-neutral-500">Balance</span>
                <span className="font-mono font-semibold text-neutral-100">
                  ${portfolio.virtualBalance.toLocaleString('en-US', { minimumFractionDigits: 2 })}
                </span>
              </div>
              <div className="flex justify-between items-center">
                <span className="text-neutral-500">Portfolio</span>
                <span className="font-mono font-semibold text-neutral-100">
                  ${portfolio.totalPortfolioValue.toLocaleString('en-US', { minimumFractionDigits: 2 })}
                </span>
              </div>
              <div className="flex justify-between items-center border-t border-neutral-800 pt-2 mt-0.5">
                <span className="text-neutral-500">P&L</span>
                <span className={`font-mono font-semibold ${pnlIsUp ? 'text-emerald-400' : 'text-red-400'}`}>
                  {pnlIsUp ? '+' : ''}${portfolio.totalPnL.toFixed(2)}{' '}
                  <span className="text-neutral-600 font-normal">({portfolio.totalPnLPercent.toFixed(2)}%)</span>
                </span>
              </div>
            </div>
          )}

          <div className="flex-1 min-h-0">
            <OrderBook
              orderBook={orderBook}
              currentPrice={selectedAsset?.currentPrice ?? 0}
              userEntryPrice={userEntryPrice}
            />
          </div>
          <ExecutionPanel selectedAsset={selectedAsset} />
        </div>
      </div>
    </div>
  );
}
