import { useMarketStore } from '../../../Application/Store/marketStore';
import { Badge } from '../../Components/UI/Badge';

export function AllAssetsPage() {
  const { assets, isLoading } = useMarketStore();

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-full text-neutral-600 text-sm animate-pulse">
        Cargando activos...
      </div>
    );
  }

  return (
    <div className="flex flex-col h-full overflow-y-auto scrollbar-thin p-6 gap-6">
      <div>
        <h2 className="text-xl font-bold text-neutral-100">Todos los activos</h2>
        <p className="text-sm text-neutral-500 mt-0.5">Mercado completo — análisis general</p>
      </div>

      <div className="bg-neutral-900 border border-neutral-800 rounded-xl overflow-hidden">
        <table className="w-full text-sm">
          <thead>
            <tr className="text-xs text-neutral-500 uppercase border-b border-neutral-800">
              {['Activo', 'Precio', 'Cambio 24h', 'Volumen 24h'].map((h) => (
                <th key={h} className="text-left px-4 py-3 font-medium">{h}</th>
              ))}
            </tr>
          </thead>
          <tbody>
            {assets.map((asset) => {
              const isUp = asset.priceChange24h >= 0;
              return (
                <tr key={asset.symbol} className="border-b border-neutral-800/50 hover:bg-neutral-800/30 transition-colors">
                  <td className="px-4 py-3">
                    <span className="font-semibold text-neutral-100">{asset.symbol}</span>
                    <span className="text-xs text-neutral-500 block">{asset.name}</span>
                  </td>
                  <td className="px-4 py-3 font-mono font-semibold text-neutral-100">
                    ${asset.currentPrice.toLocaleString('en-US', { minimumFractionDigits: 2 })}
                  </td>
                  <td className="px-4 py-3">
                    <Badge variant={isUp ? 'up' : 'down'}>
                      {isUp ? '+' : ''}{asset.priceChangePercent24h.toFixed(2)}%
                    </Badge>
                  </td>
                  <td className="px-4 py-3 font-mono text-neutral-500 text-xs">
                    ${(asset.volume24h / 1_000_000).toFixed(1)}M
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>

        {assets.length === 0 && (
          <p className="text-sm text-neutral-600 text-center py-8">No hay activos disponibles</p>
        )}
      </div>
    </div>
  );
}
