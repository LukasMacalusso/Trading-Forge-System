import { memo } from 'react';
import type { OrderBook as OrderBookData } from '../../../Domain/Entities/Asset';

interface OrderBookProps {
  orderBook: OrderBookData | null;
  currentPrice: number;
}

function Row({ price, quantity, total, maxTotal, side }: {
  price: number; quantity: number; total: number; maxTotal: number; side: 'bid' | 'ask';
}) {
  const fillPercent = (total / maxTotal) * 100;
  const isAsk = side === 'ask';

  return (
    <div className="relative flex items-center justify-between px-3 py-0.5 text-xs font-mono hover:bg-neutral-800/40 cursor-default group">
      <div
        className={`absolute inset-0 opacity-15 ${isAsk ? 'bg-red-500' : 'bg-emerald-500'}`}
        style={{ width: `${fillPercent}%`, [isAsk ? 'right' : 'left']: 0, position: 'absolute' }}
      />
      <span className={`relative z-10 font-semibold ${isAsk ? 'text-red-400' : 'text-emerald-400'}`}>
        ${price.toFixed(2)}
      </span>
      <span className="relative z-10 text-neutral-400">{quantity.toFixed(4)}</span>
      <span className="relative z-10 text-neutral-500">${total.toFixed(0)}</span>
    </div>
  );
}

/**
 * Order book showing bid/ask walls with depth visualization bars.
 * Memoized — only re-renders when order book data changes.
 */
export const OrderBook = memo(function OrderBook({ orderBook, currentPrice }: OrderBookProps) {
  if (!orderBook) {
    return (
      <div className="flex flex-col h-full bg-neutral-900 rounded-lg p-4">
        <h3 className="text-xs font-semibold text-neutral-400 uppercase tracking-wider mb-3">Order Book</h3>
        <div className="flex-1 flex items-center justify-center text-neutral-600 text-sm">Select an asset</div>
      </div>
    );
  }

  const maxBidTotal = Math.max(...orderBook.bids.map((b) => b.total));
  const maxAskTotal = Math.max(...orderBook.asks.map((a) => a.total));

  return (
    <div className="flex flex-col h-full bg-neutral-900 rounded-lg overflow-hidden">
      <div className="flex justify-between items-center px-3 py-2 border-b border-neutral-800">
        <h3 className="text-xs font-semibold text-neutral-400 uppercase tracking-wider">Order Book</h3>
        <div className="flex items-center gap-2 text-xs text-neutral-500">
          <span className="text-emerald-400">B</span>/<span className="text-red-400">A</span>
        </div>
      </div>

      <div className="flex justify-between px-3 py-1 text-xs text-neutral-600">
        <span>Price</span><span>Qty</span><span>Total</span>
      </div>

      <div className="flex-1 overflow-y-auto scrollbar-thin">
        {/* Asks (sell) — reversed so lowest ask is closest to spread */}
        <div className="flex flex-col-reverse">
          {orderBook.asks.slice(0, 10).map((ask) => (
            <Row key={ask.price} {...ask} maxTotal={maxAskTotal} side="ask" />
          ))}
        </div>

        {/* Spread */}
        <div className="flex items-center justify-between px-3 py-1.5 bg-neutral-800/60 border-y border-neutral-700">
          <span className="text-sm font-bold font-mono text-neutral-100">
            ${currentPrice.toLocaleString('en-US', { minimumFractionDigits: 2 })}
          </span>
          <span className="text-xs text-neutral-500">
            Spread ${(orderBook.asks[0]?.price - orderBook.bids[0]?.price).toFixed(2)}
          </span>
        </div>

        {/* Bids (buy) */}
        {orderBook.bids.slice(0, 10).map((bid) => (
          <Row key={bid.price} {...bid} maxTotal={maxBidTotal} side="bid" />
        ))}
      </div>
    </div>
  );
});
