import { motion } from 'framer-motion';
import { TrendingUp, TrendingDown, RotateCcw, History } from 'lucide-react';
import { usePortfolio } from '../../../Application/Handlers/usePortfolio';
import { Badge } from '../../Components/UI/Badge';
import { Button } from '../../Components/UI/Button';

export function PortfolioPage() {
  const { portfolio, orderHistory, simulationHistory, isLoading, resetSimulation } = usePortfolio();

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-full text-neutral-600 text-sm animate-pulse">
        Loading portfolio...
      </div>
    );
  }

  const pnlIsUp = (portfolio?.totalPnL ?? 0) >= 0;

  return (
    <div className="flex flex-col h-full overflow-y-auto scrollbar-thin p-6 gap-6">
      <div className="flex items-center justify-between">
        <h2 className="text-xl font-bold text-neutral-100">Portfolio</h2>
        <Button variant="danger" size="sm" onClick={resetSimulation}>
          <RotateCcw size={14} />
          Reset Simulation
        </Button>
      </div>

      {/* Summary cards */}
      {portfolio && (
        <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
          {[
            { label: 'Cash Balance', value: `$${portfolio.virtualBalance.toLocaleString('en-US', { minimumFractionDigits: 2 })}`, sub: 'Available to trade' },
            { label: 'Portfolio Value', value: `$${portfolio.totalPortfolioValue.toLocaleString('en-US', { minimumFractionDigits: 2 })}`, sub: 'Cash + positions' },
            {
              label: 'Total P&L',
              value: `${pnlIsUp ? '+' : ''}$${portfolio.totalPnL.toFixed(2)}`,
              sub: `${portfolio.totalPnLPercent.toFixed(2)}%`,
              highlight: pnlIsUp ? 'up' : 'down',
            },
            { label: 'Open Positions', value: portfolio.positions.length.toString(), sub: 'Active assets' },
          ].map(({ label, value, sub, highlight }) => (
            <motion.div
              key={label}
              initial={{ opacity: 0, y: 8 }}
              animate={{ opacity: 1, y: 0 }}
              className="bg-neutral-900 border border-neutral-800 rounded-xl p-4"
            >
              <p className="text-xs text-neutral-500 uppercase tracking-wider">{label}</p>
              <p className={`text-2xl font-bold font-mono mt-1 ${highlight === 'up' ? 'text-emerald-400' : highlight === 'down' ? 'text-red-400' : 'text-neutral-100'}`}>
                {value}
              </p>
              <p className="text-xs text-neutral-600 mt-0.5">{sub}</p>
            </motion.div>
          ))}
        </div>
      )}

      {/* Positions */}
      <div className="bg-neutral-900 border border-neutral-800 rounded-xl overflow-hidden">
        <div className="px-4 py-3 border-b border-neutral-800 flex items-center gap-2">
          <TrendingUp size={14} className="text-neutral-400" />
          <h3 className="text-sm font-semibold text-neutral-200">Open Positions</h3>
        </div>
        {portfolio?.positions.length === 0 ? (
          <p className="text-sm text-neutral-600 text-center py-8">No open positions</p>
        ) : (
          <table className="w-full text-sm">
            <thead>
              <tr className="text-xs text-neutral-500 uppercase border-b border-neutral-800">
                {['Asset', 'Qty', 'Avg Price', 'Current', 'P&L', 'Value'].map((h) => (
                  <th key={h} className="text-left px-4 py-2 font-medium">{h}</th>
                ))}
              </tr>
            </thead>
            <tbody>
              {portfolio?.positions.map((pos) => {
                const isUp = pos.unrealizedPnL >= 0;
                return (
                  <motion.tr
                    key={pos.symbol}
                    initial={{ opacity: 0 }}
                    animate={{ opacity: 1 }}
                    className="border-b border-neutral-800/50 hover:bg-neutral-800/30 transition-colors"
                  >
                    <td className="px-4 py-3">
                      <span className="font-semibold text-neutral-100">{pos.symbol}</span>
                      <span className="text-xs text-neutral-500 block">{pos.assetName}</span>
                    </td>
                    <td className="px-4 py-3 font-mono text-neutral-300">{pos.quantity}</td>
                    <td className="px-4 py-3 font-mono text-neutral-400">${pos.averageBuyPrice.toFixed(2)}</td>
                    <td className="px-4 py-3 font-mono text-neutral-100">${pos.currentPrice.toFixed(2)}</td>
                    <td className="px-4 py-3">
                      <Badge variant={isUp ? 'up' : 'down'}>
                        {isUp ? '+' : ''}${pos.unrealizedPnL.toFixed(2)} ({pos.unrealizedPnLPercent.toFixed(2)}%)
                      </Badge>
                    </td>
                    <td className="px-4 py-3 font-mono text-neutral-200">${pos.totalValue.toFixed(2)}</td>
                  </motion.tr>
                );
              })}
            </tbody>
          </table>
        )}
      </div>

      {/* Order History */}
      <div className="bg-neutral-900 border border-neutral-800 rounded-xl overflow-hidden">
        <div className="px-4 py-3 border-b border-neutral-800 flex items-center gap-2">
          <History size={14} className="text-neutral-400" />
          <h3 className="text-sm font-semibold text-neutral-200">Order History</h3>
        </div>
        {orderHistory.length === 0 ? (
          <p className="text-sm text-neutral-600 text-center py-8">No orders yet</p>
        ) : (
          <table className="w-full text-sm">
            <thead>
              <tr className="text-xs text-neutral-500 uppercase border-b border-neutral-800">
                {['Asset', 'Side', 'Type', 'Qty', 'Price', 'Total', 'Status', 'Date'].map((h) => (
                  <th key={h} className="text-left px-4 py-2 font-medium">{h}</th>
                ))}
              </tr>
            </thead>
            <tbody>
              {orderHistory.map((order) => (
                <tr key={order.id} className="border-b border-neutral-800/50 hover:bg-neutral-800/30">
                  <td className="px-4 py-2 font-semibold text-neutral-100">{order.symbol}</td>
                  <td className="px-4 py-2">
                    <Badge variant={order.side === 'Buy' ? 'up' : 'down'}>{order.side}</Badge>
                  </td>
                  <td className="px-4 py-2 text-neutral-500">{order.type}</td>
                  <td className="px-4 py-2 font-mono text-neutral-300">{order.quantity}</td>
                  <td className="px-4 py-2 font-mono text-neutral-400">${order.price.toFixed(2)}</td>
                  <td className="px-4 py-2 font-mono text-neutral-200">${order.total.toFixed(2)}</td>
                  <td className="px-4 py-2">
                    <Badge variant={order.status === 'Filled' ? 'up' : order.status === 'Cancelled' ? 'down' : 'neutral'}>
                      {order.status}
                    </Badge>
                  </td>
                  <td className="px-4 py-2 text-xs text-neutral-600">
                    {new Date(order.createdAt).toLocaleDateString()}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      {/* Simulation History */}
      {simulationHistory.length > 0 && (
        <div className="bg-neutral-900 border border-neutral-800 rounded-xl overflow-hidden">
          <div className="px-4 py-3 border-b border-neutral-800 flex items-center gap-2">
            <TrendingDown size={14} className="text-neutral-400" />
            <h3 className="text-sm font-semibold text-neutral-200">Past Simulations</h3>
          </div>
          <div className="divide-y divide-neutral-800">
            {simulationHistory.map((snap) => (
              <div key={snap.id} className="px-4 py-3 flex items-center justify-between hover:bg-neutral-800/30">
                <div>
                  <p className="text-sm text-neutral-200 font-mono">${snap.finalPortfolioValue.toLocaleString()}</p>
                  <p className="text-xs text-neutral-500">{new Date(snap.createdAt).toLocaleDateString()}</p>
                </div>
                <Badge variant={snap.totalPnL >= 0 ? 'up' : 'down'}>
                  {snap.totalPnL >= 0 ? '+' : ''}{snap.totalPnLPercent.toFixed(2)}%
                </Badge>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}
