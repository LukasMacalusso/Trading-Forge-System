import { useState } from 'react';
import { motion } from 'framer-motion';
import type { Asset } from '@models/Asset';
import type { OrderSide, OrderType } from '@models/Order';
import { usePlaceOrder } from '@hooks/usePlaceOrder';
import { usePortfolioStore } from '@store/portfolioStore';
import { COMMISSION_RATE } from '@utils/constants';
import { Button } from '../UI/Button';
import { Input } from '../UI/Input';

interface ExecutionPanelProps {
  selectedAsset: Asset | null;
}


export function ExecutionPanel({ selectedAsset }: ExecutionPanelProps) {
  const [side, setSide] = useState<OrderSide>('Buy');
  const [orderType, setOrderType] = useState<OrderType>('Market');
  const [quantity, setQuantity] = useState('');
  const [limitPrice, setLimitPrice] = useState('');

  const { placeOrder, isSubmitting } = usePlaceOrder();
  const { portfolio } = usePortfolioStore();

  const currentPosition = selectedAsset
    ? portfolio?.positions.find((p) => p.symbol === selectedAsset.symbol)
    : undefined;

  const price = orderType === 'Market'
    ? (selectedAsset?.currentPrice ?? 0)
    : (parseFloat(limitPrice) || 0);

  const qty = side === 'Sell'
    ? (currentPosition?.quantity ?? 0)
    : (parseFloat(quantity) || 0);
  const commission = qty * price * COMMISSION_RATE;
  const estimatedTotal = side === 'Buy' ? qty * price + commission : qty * price - commission;

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!selectedAsset) return;
    if (side === 'Buy' && qty <= 0) return;
    if (side === 'Sell' && !currentPosition) return;

    const success = await placeOrder(
      {
        traderId: '',
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
      <h3 className="text-xs font-semibold text-neutral-400 uppercase tracking-wider">Ejecutar Orden</h3>

      {!selectedAsset && (
        <p className="text-sm text-neutral-600 text-center py-4">Selecciona un activo para operar</p>
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
                    ? s === 'Buy' ? 'bg-amber-500 text-neutral-950' : 'bg-red-500 text-white'
                    : 'text-neutral-500 hover:text-neutral-300'
                }`}
              >
                {s === 'Buy' ? 'Comprar' : 'Vender'}
              </button>
            ))}
          </div>

          {/* BUY side — quantity + order type */}
          {side === 'Buy' && (
            <>
              <div className="flex gap-2">
                {(['Market', 'Limit'] as OrderType[]).map((t) => (
                  <button
                    key={t}
                    type="button"
                    onClick={() => setOrderType(t)}
                    className={`px-3 py-1 text-xs rounded-md border transition-colors ${
                      orderType === t
                        ? 'border-amber-500 text-amber-400 bg-amber-500/10'
                        : 'border-neutral-700 text-neutral-500 hover:border-neutral-600'
                    }`}
                  >
                    {t}
                  </button>
                ))}
              </div>

              <Input
                label="Cantidad"
                type="number"
                min="0"
                step="any"
                placeholder="0.00"
                value={quantity}
                onChange={(e) => setQuantity(e.target.value)}
              />

              {orderType === 'Limit' && (
                <Input
                  label="Precio límite"
                  type="number"
                  min="0"
                  step="any"
                  placeholder={selectedAsset.currentPrice.toFixed(2)}
                  value={limitPrice}
                  onChange={(e) => setLimitPrice(e.target.value)}
                />
              )}
            </>
          )}

          {/* SELL side — shows full position info */}
          {side === 'Sell' && (
            currentPosition ? (
              <div className="text-xs space-y-1.5 p-3 bg-neutral-800/60 rounded-lg border border-neutral-700">
                <div className="flex justify-between text-neutral-400">
                  <span>Cantidad</span>
                  <span className="font-mono text-neutral-200">{currentPosition.quantity}</span>
                </div>
                <div className="flex justify-between text-neutral-400">
                  <span>Precio entrada</span>
                  <span className="font-mono">${currentPosition.averageBuyPrice.toFixed(2)}</span>
                </div>
                <div className="flex justify-between text-neutral-400">
                  <span>P&L no realizado</span>
                  <span className={`font-mono font-semibold ${currentPosition.unrealizedPnL >= 0 ? 'text-emerald-400' : 'text-red-400'}`}>
                    {currentPosition.unrealizedPnL >= 0 ? '+' : ''}${currentPosition.unrealizedPnL.toFixed(2)}
                  </span>
                </div>
                <p className="text-neutral-600 pt-1 border-t border-neutral-700">Cierra la posición completa al precio de mercado</p>
              </div>
            ) : (
              <p className="text-xs text-neutral-600 text-center py-2">No tienes posición en {selectedAsset.symbol}</p>
            )
          )}

          {/* Buy summary */}
          {side === 'Buy' && qty > 0 && (
            <motion.div
              initial={{ opacity: 0, y: -4 }}
              animate={{ opacity: 1, y: 0 }}
              className="text-xs space-y-1 p-3 bg-neutral-800 rounded-lg border border-neutral-700"
            >
              <div className="flex justify-between text-neutral-400">
                <span>Precio</span>
                <span className="font-mono">${price.toFixed(2)}</span>
              </div>
              <div className="flex justify-between text-neutral-400">
                <span>Comisión (0.1%)</span>
                <span className="font-mono">${commission.toFixed(2)}</span>
              </div>
              <div className="flex justify-between text-neutral-100 font-semibold border-t border-neutral-700 pt-1 mt-1">
                <span>Total</span>
                <span className="font-mono">${estimatedTotal.toFixed(2)}</span>
              </div>
              <div className="flex justify-between text-neutral-500">
                <span>Balance tras orden</span>
                <span className={`font-mono ${(portfolio?.virtualBalance ?? 0) - estimatedTotal < 0 ? 'text-red-400' : ''}`}>
                  ${((portfolio?.virtualBalance ?? 0) - estimatedTotal).toFixed(2)}
                </span>
              </div>
            </motion.div>
          )}

          <Button
            type="submit"
            isLoading={isSubmitting}
            disabled={side === 'Buy' ? qty <= 0 : !currentPosition}
            variant={side === 'Buy' ? 'primary' : 'danger'}
            className={side === 'Sell' ? 'bg-red-500 hover:bg-red-400 text-white border-0' : ''}
          >
            {side === 'Buy' ? `Comprar ${selectedAsset.symbol}` : `Cerrar posición ${selectedAsset.symbol}`}
          </Button>
        </form>
      )}
    </div>
  );
}
