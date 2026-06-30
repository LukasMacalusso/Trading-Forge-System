import { X, Receipt, TrendingUp } from 'lucide-react';
import type { SimulationSnapshot, Position, Transaction } from '@models/Portfolio';
import { Badge } from '@components/UI/Badge';

interface SimulationDetailModalProps {
  snapshot: SimulationSnapshot;
  positions: Position[];
  transactions: Transaction[];
  loading: boolean;
  onClose: () => void;
}

export function SimulationDetailModal({
  snapshot,
  positions,
  transactions,
  loading,
  onClose,
}: SimulationDetailModalProps) {
  const pnlUp = snapshot.totalPnL >= 0;

  return (
    <div className="fixed inset-0 z-[1000] flex items-center justify-center p-4">
      <div className="absolute inset-0 bg-black/70" onClick={onClose} />
      <div className="relative w-full max-w-2xl max-h-[85vh] overflow-y-auto scrollbar-thin bg-neutral-900 border border-neutral-700 rounded-2xl shadow-2xl p-5 flex flex-col gap-4">
        <div className="flex items-start justify-between">
          <div>
            <h3 className="text-base font-semibold text-neutral-100">Simulación pasada</h3>
            <p className="text-xs text-neutral-500 mt-0.5">
              {new Date(snapshot.createdAt).toLocaleString()}
            </p>
          </div>
          <button
            onClick={onClose}
            aria-label="Cerrar"
            className="p-1 -m-1 text-neutral-500 hover:text-neutral-300"
          >
            <X size={16} />
          </button>
        </div>

        <div className="grid grid-cols-2 gap-3">
          <div className="bg-neutral-800/50 rounded-lg p-3">
            <p className="text-xs text-neutral-500">Balance final</p>
            <p className="text-lg font-mono font-bold text-neutral-100">
              ${snapshot.finalBalance.toLocaleString('en-US', { minimumFractionDigits: 2 })}
            </p>
          </div>
          <div className="bg-neutral-800/50 rounded-lg p-3">
            <p className="text-xs text-neutral-500">P&L</p>
            <p className={`text-lg font-mono font-bold ${pnlUp ? 'text-emerald-400' : 'text-red-400'}`}>
              {pnlUp ? '+' : ''}${snapshot.totalPnL.toFixed(2)}{' '}
              <span className="text-sm font-normal text-neutral-500">
                ({snapshot.totalPnLPercent.toFixed(2)}%)
              </span>
            </p>
          </div>
        </div>

        {loading ? (
          <div className="py-12 text-center text-sm text-neutral-600 animate-pulse">
            Cargando detalle...
          </div>
        ) : (
          <>
            <Section icon={<TrendingUp size={14} className="text-neutral-400" />} title="Posiciones">
              {positions.length === 0 ? (
                <Empty>No había posiciones abiertas</Empty>
              ) : (
                <table className="w-full text-sm">
                  <thead>
                    <tr className="text-xs text-neutral-500 uppercase border-b border-neutral-800">
                      {['Activo', 'Cantidad', 'Precio', 'Valor'].map((h) => (
                        <th key={h} className="text-left px-3 py-2 font-medium">{h}</th>
                      ))}
                    </tr>
                  </thead>
                  <tbody>
                    {positions.map((p) => (
                      <tr key={p.id} className="border-b border-neutral-800/50">
                        <td className="px-3 py-2 font-semibold text-neutral-100">{p.symbol}</td>
                        <td className="px-3 py-2 font-mono text-neutral-300">{p.quantity}</td>
                        <td className="px-3 py-2 font-mono text-neutral-400">${p.averageBuyPrice.toFixed(2)}</td>
                        <td className="px-3 py-2 font-mono text-neutral-200">${p.totalValue.toFixed(2)}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              )}
            </Section>

            <Section icon={<Receipt size={14} className="text-neutral-400" />} title="Movimientos">
              {transactions.length === 0 ? (
                <Empty>Sin movimientos registrados</Empty>
              ) : (
                <table className="w-full text-sm">
                  <thead>
                    <tr className="text-xs text-neutral-500 uppercase border-b border-neutral-800">
                      {['Tipo', 'Activo', 'Total', 'Saldo', 'Fecha'].map((h) => (
                        <th key={h} className="text-left px-3 py-2 font-medium">{h}</th>
                      ))}
                    </tr>
                  </thead>
                  <tbody>
                    {transactions.map((tx) => (
                      <tr key={tx.id} className="border-b border-neutral-800/50">
                        <td className="px-3 py-2">
                          <Badge variant={tx.type === 'Buy' ? 'down' : tx.type === 'Sell' ? 'up' : 'neutral'}>
                            {tx.type}
                          </Badge>
                        </td>
                        <td className="px-3 py-2 font-semibold text-neutral-100">{tx.symbol ?? '—'}</td>
                        <td className="px-3 py-2 font-mono text-neutral-200">${tx.total.toFixed(2)}</td>
                        <td className="px-3 py-2 font-mono text-neutral-300">${tx.balanceAfter.toFixed(2)}</td>
                        <td className="px-3 py-2 text-xs text-neutral-600">
                          {new Date(tx.createdAt).toLocaleDateString()}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              )}
            </Section>
          </>
        )}
      </div>
    </div>
  );
}

function Section({ icon, title, children }: { icon: React.ReactNode; title: string; children: React.ReactNode }) {
  return (
    <div className="border border-neutral-800 rounded-xl overflow-hidden">
      <div className="px-3 py-2 border-b border-neutral-800 flex items-center gap-2">
        {icon}
        <h4 className="text-sm font-semibold text-neutral-200">{title}</h4>
      </div>
      <div className="overflow-x-auto scrollbar-thin">{children}</div>
    </div>
  );
}

function Empty({ children }: { children: React.ReactNode }) {
  return <p className="text-sm text-neutral-600 text-center py-6">{children}</p>;
}
