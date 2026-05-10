import { useEffect } from 'react';
import { AlertTriangle } from 'lucide-react';
import { useMarketData } from '../../../Application/Handlers/useMarketData';
import { usePortfolioStore } from '../../../Application/Store/portfolioStore';
import { usePortfolio } from '../../../Application/Handlers/usePortfolio';
import { PriceTicker } from '../../Components/Ticker/PriceTicker';
import { CandlestickChart } from '../../Components/Charts/CandlestickChart';
import { OrderBook } from '../../Components/OrderBook/OrderBook';
import { ExecutionPanel } from '../../Components/Orders/ExecutionPanel';
import { Badge } from '../../Components/UI/Badge';

export function DashboardPage() {
  const { assets, selectedAsset, candles, orderBook, isLoading, isStale, handleSelectAsset } = useMarketData();
  const { portfolio } = usePortfolioStore();
  usePortfolio();

  useEffect(() => {
    if (assets.length > 0 && !selectedAsset) {
      handleSelectAsset(assets[0]);
    }
  }, [assets]);

  const pnlIsUp = (portfolio?.totalPnL ?? 0) >= 0;

  return (
    <div className="flex flex-col h-full min-h-0">
      {/* Stale data warning — BR-19 */}
      {isStale && (
        <div className="flex items-center gap-2 px-4 py-2 bg-amber-500/10 border-b border-amber-500/20 text-amber-400 text-xs">
          <AlertTriangle size={14} />
          Market data may be delayed. Refresh if prices seem incorrect.
        </div>
      )}

      {/* Price Ticker */}
      {isLoading ? (
        <div className="h-14 border-b border-neutral-800 flex items-center px-4">
          <span className="text-xs text-neutral-600 animate-pulse">Loading market data...</span>
        </div>
      ) : (
        <PriceTicker
          assets={assets}
          selectedSymbol={selectedAsset?.symbol}
          onSelect={handleSelectAsset}
        />
      )}

      {/* Main content */}
      <div className="flex flex-1 min-h-0 gap-0">
        {/* Center: Chart + header */}
        <div className="flex flex-col flex-1 min-w-0 min-h-0 p-3 gap-3">
          {/* Asset header */}
          {selectedAsset && (
            <div className="flex items-center gap-4 px-1">
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
              <span className="text-xs text-neutral-600 ml-auto">
                Vol: {(selectedAsset.volume24h / 1_000_000).toFixed(1)}M
              </span>
            </div>
          )}

          {/* Candlestick Chart */}
          <div className="flex-1 min-h-0 bg-[#0a0a0b] rounded-lg overflow-hidden relative">
            <div className="absolute inset-0">
              <CandlestickChart candles={candles} symbol={selectedAsset?.symbol ?? ''} />
            </div>
          </div>

          {/* Portfolio summary bar */}
          {portfolio && (
            <div className="flex items-center gap-6 px-3 py-2 bg-neutral-900 rounded-lg border border-neutral-800 text-xs">
              <div>
                <span className="text-neutral-500">Balance </span>
                <span className="font-mono font-semibold text-neutral-100">
                  ${portfolio.virtualBalance.toLocaleString('en-US', { minimumFractionDigits: 2 })}
                </span>
              </div>
              <div>
                <span className="text-neutral-500">Portfolio </span>
                <span className="font-mono font-semibold text-neutral-100">
                  ${portfolio.totalPortfolioValue.toLocaleString('en-US', { minimumFractionDigits: 2 })}
                </span>
              </div>
              <div>
                <span className="text-neutral-500">Total P&L </span>
                <span className={`font-mono font-semibold ${pnlIsUp ? 'text-emerald-400' : 'text-red-400'}`}>
                  {pnlIsUp ? '+' : ''}${portfolio.totalPnL.toFixed(2)} ({portfolio.totalPnLPercent.toFixed(2)}%)
                </span>
              </div>
            </div>
          )}
        </div>

        {/* Right panel: Order Book + Execution */}
        <div className="w-72 shrink-0 flex flex-col gap-3 p-3 border-l border-neutral-800">
          <div className="flex-1 min-h-0">
            <OrderBook orderBook={orderBook} currentPrice={selectedAsset?.currentPrice ?? 0} />
          </div>
          <ExecutionPanel selectedAsset={selectedAsset} />
        </div>
      </div>
    </div>
  );
}
