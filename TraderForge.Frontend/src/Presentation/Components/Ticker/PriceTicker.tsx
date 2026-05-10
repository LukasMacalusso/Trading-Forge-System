import { memo, useRef, useEffect, useState } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import type { Asset } from '../../../Domain/Entities/Asset';

interface PriceTickerProps {
  assets: Asset[];
  selectedSymbol?: string;
  onSelect: (asset: Asset) => void;
}

function TickerItem({ asset, isSelected, onSelect }: { asset: Asset; isSelected: boolean; onSelect: () => void }) {
  const prevPrice = useRef(asset.currentPrice);
  const [flash, setFlash] = useState<'up' | 'down' | null>(null);
  const isUp = asset.priceChange24h >= 0;

  useEffect(() => {
    if (asset.currentPrice !== prevPrice.current) {
      setFlash(asset.currentPrice > prevPrice.current ? 'up' : 'down');
      prevPrice.current = asset.currentPrice;
      const t = setTimeout(() => setFlash(null), 600);
      return () => clearTimeout(t);
    }
  }, [asset.currentPrice]);

  return (
    <button
      onClick={onSelect}
      className={`flex flex-col items-start px-4 py-2 border-r border-neutral-800 shrink-0 transition-colors hover:bg-neutral-900 ${isSelected ? 'bg-neutral-900 border-b-2 border-b-emerald-500' : ''}`}
    >
      <div className="flex items-center gap-2">
        <span className="text-xs font-bold text-neutral-100">{asset.symbol}</span>
        <span className={`text-xs ${isUp ? 'text-emerald-400' : 'text-red-400'}`}>
          {isUp ? '+' : ''}{asset.priceChangePercent24h.toFixed(2)}%
        </span>
      </div>
      <AnimatePresence mode="wait">
        <motion.span
          key={asset.currentPrice}
          initial={{ opacity: 0.6 }}
          animate={{ opacity: 1 }}
          className={`text-sm font-mono font-semibold transition-colors duration-300 ${
            flash === 'up' ? 'text-emerald-400' : flash === 'down' ? 'text-red-400' : 'text-neutral-100'
          }`}
        >
          ${asset.currentPrice.toLocaleString('en-US', { minimumFractionDigits: 2 })}
        </motion.span>
      </AnimatePresence>
    </button>
  );
}

/**
 * Horizontal scrollable price ticker bar.
 * Memoized to prevent re-renders of the full list when only one price changes.
 */
export const PriceTicker = memo(function PriceTicker({ assets, selectedSymbol, onSelect }: PriceTickerProps) {
  return (
    <div className="flex overflow-x-auto scrollbar-thin border-b border-neutral-800 bg-neutral-950">
      {assets.map((asset) => (
        <TickerItem
          key={asset.symbol}
          asset={asset}
          isSelected={asset.symbol === selectedSymbol}
          onSelect={() => onSelect(asset)}
        />
      ))}
    </div>
  );
});
