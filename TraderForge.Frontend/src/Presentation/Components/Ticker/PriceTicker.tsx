import { memo, useRef, useEffect, useState } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { Plus, X } from 'lucide-react';
import type { Asset } from '../../../Domain/Entities/Asset';

interface PriceTickerProps {
  assets: Asset[];
  allAssets: Asset[];
  selectedSymbol?: string;
  onSelect: (asset: Asset) => void;
  onAdd: (symbol: string) => void;
  onRemove: (symbol: string) => void;
}

function TickerItem({
  asset,
  isSelected,
  onSelect,
  onRemove,
}: {
  asset: Asset;
  isSelected: boolean;
  onSelect: () => void;
  onRemove: () => void;
}) {
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
    <div className={`group relative flex items-stretch border-r border-neutral-800 shrink-0 ${isSelected ? 'bg-neutral-900 border-b-2 border-b-amber-500' : ''}`}>
      <button
        onClick={onSelect}
        className="flex flex-col items-start px-4 py-2 transition-colors hover:bg-neutral-900 pr-7"
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

      {/* Remove button — visible on hover */}
      <button
        onClick={(e) => { e.stopPropagation(); onRemove(); }}
        className="absolute right-1 top-1/2 -translate-y-1/2 w-4 h-4 flex items-center justify-center rounded opacity-0 group-hover:opacity-100 transition-opacity text-neutral-600 hover:text-red-400"
      >
        <X size={10} strokeWidth={2.5} />
      </button>
    </div>
  );
}

function AddAssetDropdown({ assets, onAdd, onClose }: { assets: Asset[]; onAdd: (symbol: string) => void; onClose: () => void }) {
  const ref = useRef<HTMLDivElement>(null);

  useEffect(() => {
    function handleClick(e: MouseEvent) {
      if (ref.current && !ref.current.contains(e.target as Node)) onClose();
    }
    document.addEventListener('mousedown', handleClick);
    return () => document.removeEventListener('mousedown', handleClick);
  }, [onClose]);

  return (
    <div ref={ref} className="absolute top-full right-0 mt-1 w-56 bg-neutral-900 border border-neutral-700 rounded-xl shadow-xl z-50 overflow-hidden">
      {assets.length === 0 ? (
        <p className="text-xs text-neutral-500 px-3 py-3 text-center">All assets added</p>
      ) : (
        <div className="max-h-60 overflow-y-auto">
          {assets.map((a) => (
            <button
              key={a.symbol}
              onClick={() => { onAdd(a.symbol); onClose(); }}
              className="w-full flex items-center justify-between px-3 py-2 text-sm hover:bg-neutral-800 transition-colors"
            >
              <span className="font-semibold text-neutral-100">{a.symbol}</span>
              <span className="text-xs text-neutral-500">{a.name}</span>
            </button>
          ))}
        </div>
      )}
    </div>
  );
}

export const PriceTicker = memo(function PriceTicker({
  assets,
  allAssets,
  selectedSymbol,
  onSelect,
  onAdd,
  onRemove,
}: PriceTickerProps) {
  const [dropdownOpen, setDropdownOpen] = useState(false);
  const unwatched = allAssets.filter((a) => !assets.some((w) => w.symbol === a.symbol));

  return (
    <div className="flex border-b border-neutral-800 bg-neutral-950">
      {/* Scrollable ticker items — isolated so overflow-x-auto doesn't clip the dropdown */}
      <div className="flex overflow-x-auto scrollbar-thin flex-1 min-w-0">
        {assets.map((asset) => (
          <TickerItem
            key={asset.symbol}
            asset={asset}
            isSelected={asset.symbol === selectedSymbol}
            onSelect={() => onSelect(asset)}
            onRemove={() => onRemove(asset.symbol)}
          />
        ))}
      </div>

      {/* Add asset button — sibling of scroll container so dropdown renders outside its clip area */}
      <div className="relative flex items-center px-3 shrink-0 border-l border-neutral-800">
        <button
          onClick={() => setDropdownOpen((o) => !o)}
          className="flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-semibold bg-amber-500/10 text-amber-400 border border-amber-500/30 hover:bg-amber-500/20 transition-colors"
        >
          <Plus size={12} strokeWidth={2.5} />
          Añadir
        </button>
        {dropdownOpen && (
          <AddAssetDropdown
            assets={unwatched}
            onAdd={onAdd}
            onClose={() => setDropdownOpen(false)}
          />
        )}
      </div>
    </div>
  );
});
