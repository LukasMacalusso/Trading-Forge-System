import { useState } from 'react';
import { motion } from 'framer-motion';
import type { Asset } from '../../../Domain/Entities/Asset';
import type { OrderSide, OrderType } from '../../../Domain/Entities/Order';
import { usePlaceOrder } from '../../../Application/Handlers/usePlaceOrder';
import { usePortfolioStore } from '../../../Application/Store/portfolioStore';
import { COMMISSION_RATE } from '../../../Application/Common/constants';
import { Button } from '../UI/Button';
import { Input } from '../UI/Input';

interface ExecutionPanelProps {
  selectedAsset: Asset | null;
}

/**
 * Order entry panel supporting Market and Limit order types.
 * Validates balance (BR-1, BR-6) before submission.
 */
export function ExecutionPanel({ selectedAsset }: ExecutionPanelProps) {
  const [side, setSide] = useState<OrderSide>('Buy');
  const [orderType, setOrderType] = useState<OrderType>('Market');
  const [quantity, setQuantity] = useState('');
  const [limitPrice, setLimitPrice] = useState('');

  const { placeOrder, isSubmitting } = usePlaceOrder();
  const { portfolio } = usePortfolioStore();

  const price = orderType === 'Market'
    ? (selectedAsset?.currentPrice ?? 0)
    : (parseFloat(limitPrice) || 0);

  const qty = parseFloat(quantity) || 0;
  const commission = qty * price * COMMISSION_RATE;
  const estimatedTotal = side === 'Buy' ? qty * price + commission : qty * price - commission;

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!selectedAsset || qty <= 0) return;

    const success = await placeOrder(
      {
        traderId: 'mock-trader-id',
        symbol: selectedAsset.symbol,
        side,
        type: orderType,
        quantity: qty,
        limitPrice: orderType === 'Limit' ? parseFloat(limitPrice) : undefined,
      },
      selectedAsset.currentPrice
    );

    if (success) {
      setQuantity('');
      setLimitPrice('');
    }
  }

  return (
    <div className="bg-neutral-900 rounded-lg p-4 flex flex-col gap-4">
      <h3 className="text-xs font-semibold text-neutral-400 uppercase tracking-wider">Execute Order</h3>

      {!selectedAsset && (
        <p className="text-sm text-neutral-600 text-center py-4">Select an asset to trade</p>
      )}

      {selectedAsset && (
        <form onSubmit={handleSubmit} className="flex flex-col gap-4">
          {/* Buy / Sell toggle */}
          <div className="flex rounded-lg overflow-hidden border border-neutral-800">
            {(['Buy', 'Sell'] as OrderSide[]).map((s) => (
              <button
                key={s}
                type="button"
                onClick={() => setSide(s)}
                className={`flex-1 py-2 text-sm font-semibold transition-colors ${
                  side === s
                    ? s === 'Buy' ? 'bg-emerald-500 text-neutral-950' : 'bg-red-500 text-white'
                    : 'text-neutral-500 hover:text-neutral-300'
                }`}
              >
                {s}
              </button>
            ))}
          </div>

          {/* Order type toggle */}
          <div className="flex gap-2">
            {(['Market', 'Limit'] as OrderType[]).map((t) => (
              <button
                key={t}
                type="button"
                onClick={() => setOrderType(t)}
                className={`px-3 py-1 text-xs rounded-md border transition-colors ${
                  orderType === t
                    ? 'border-emerald-500 text-emerald-400 bg-emerald-500/10'
                    : 'border-neutral-700 text-neutral-500 hover:border-neutral-600'
                }`}
              >
                {t}
              </button>
            ))}
          </div>

          <Input
            label="Quantity"
            type="number"
            min="0"
            step="any"
            placeholder="0.00"
            value={quantity}
            onChange={(e) => setQuantity(e.target.value)}
          />

          {orderType === 'Limit' && (
            <Input
              label="Limit Price"
              type="number"
              min="0"
              step="any"
              placeholder={selectedAsset.currentPrice.toFixed(2)}
              value={limitPrice}
              onChange={(e) => setLimitPrice(e.target.value)}
            />
          )}

          {/* Summary */}
          {qty > 0 && (
            <motion.div
              initial={{ opacity: 0, y: -4 }}
              animate={{ opacity: 1, y: 0 }}
              className="text-xs space-y-1 p-3 bg-neutral-800 rounded-lg border border-neutral-700"
            >
              <div className="flex justify-between text-neutral-400">
                <span>Price</span>
                <span className="font-mono">${price.toFixed(2)}</span>
              </div>
              <div className="flex justify-between text-neutral-400">
                <span>Commission (0.1%)</span>
                <span className="font-mono">${commission.toFixed(2)}</span>
              </div>
              <div className="flex justify-between text-neutral-100 font-semibold border-t border-neutral-700 pt-1 mt-1">
                <span>Total</span>
                <span className="font-mono">${estimatedTotal.toFixed(2)}</span>
              </div>
              <div className="flex justify-between text-neutral-500">
                <span>Balance after</span>
                <span className={`font-mono ${(portfolio?.virtualBalance ?? 0) - (side === 'Buy' ? estimatedTotal : 0) < 0 ? 'text-red-400' : ''}`}>
                  ${((portfolio?.virtualBalance ?? 0) - (side === 'Buy' ? estimatedTotal : -estimatedTotal)).toFixed(2)}
                </span>
              </div>
            </motion.div>
          )}

          <Button
            type="submit"
            isLoading={isSubmitting}
            disabled={!qty || qty <= 0}
            variant={side === 'Buy' ? 'primary' : 'danger'}
            className={side === 'Sell' ? 'bg-red-500 hover:bg-red-400 text-white border-0' : ''}
          >
            {side === 'Buy' ? 'Buy' : 'Sell'} {selectedAsset.symbol}
          </Button>
        </form>
      )}
    </div>
  );
}
