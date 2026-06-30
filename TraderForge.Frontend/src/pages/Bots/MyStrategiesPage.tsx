import { useCallback, useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Plus,
  Workflow,
  Play,
  Pause,
  Copy,
  Trash2,
  Activity,
  Bell,
  Zap,
  Loader2,
} from 'lucide-react';
import { StrategyService } from '@api/StrategyService';
import type { Strategy, StrategyStatus } from '@models/Strategy';
import type { BotNodeKind } from '@models/BotFlow';
import { useNotificationStore } from '@store/notificationStore';

const strategyService = new StrategyService();

const STATUS_META: Record<StrategyStatus, { label: string; cls: string }> = {
  active: { label: 'Activa', cls: 'bg-emerald-500/10 border-emerald-500/30 text-emerald-400' },
  paused: { label: 'Pausada', cls: 'bg-amber-500/10 border-amber-500/30 text-amber-400' },
  draft: { label: 'Borrador', cls: 'bg-neutral-800 border-neutral-700 text-neutral-400' },
};

const BOT_META: Record<BotNodeKind, { icon: typeof Activity; color: string }> = {
  analysisBot: { icon: Activity, color: 'text-blue-400' },
  notificationBot: { icon: Bell, color: 'text-amber-400' },
  actionBot: { icon: Zap, color: 'text-emerald-400' },
};

function countBots(strategy: Strategy): Record<BotNodeKind, number> {
  const counts: Record<BotNodeKind, number> = { analysisBot: 0, notificationBot: 0, actionBot: 0 };
  for (const node of strategy.nodes) counts[node.data.kind] += 1;
  return counts;
}

function relativeTime(iso: string): string {
  const diff = Date.now() - new Date(iso).getTime();
  const mins = Math.floor(diff / 60000);
  if (mins < 1) return 'hace un momento';
  if (mins < 60) return `hace ${mins} min`;
  const hours = Math.floor(mins / 60);
  if (hours < 24) return `hace ${hours} h`;
  const days = Math.floor(hours / 24);
  if (days < 30) return `hace ${days} d`;
  return new Date(iso).toLocaleDateString('es');
}

export function MyStrategiesPage() {
  const navigate = useNavigate();
  const addNotification = useNotificationStore((s) => s.addNotification);
  const [strategies, setStrategies] = useState<Strategy[]>([]);
  const [loading, setLoading] = useState(true);

  const refresh = useCallback(async () => {
    const result = await strategyService.list();
    if (result.isSuccess) setStrategies(result.value!);
    else if (result.errorMessage !== 'UNAUTHORIZED') {
      addNotification('error', result.errorMessage ?? 'No se pudieron cargar las estrategias.');
    }
    setLoading(false);
  }, [addNotification]);

  useEffect(() => {
    refresh();
  }, [refresh]);

  async function handleCreate() {
    const result = await strategyService.create('Nueva estrategia');
    if (result.isSuccess) navigate(`/strategy/${result.value!.id}`);
    else addNotification('error', result.errorMessage ?? 'No se pudo crear la estrategia.');
  }

  async function toggleStatus(strategy: Strategy) {
    if (strategy.status !== 'active' && strategy.nodes.length === 0) {
      addNotification('warning', 'Agrega bots a la estrategia antes de activarla');
      return;
    }
    const isActivating = strategy.status !== 'active';
    const result = isActivating
      ? await strategyService.startEngine(strategy.id)
      : await strategyService.stopEngine(strategy.id);
    if (result.isSuccess) {
      addNotification('success', isActivating ? 'Estrategia activada' : 'Estrategia pausada');
      refresh();
    } else {
      addNotification('error', result.errorMessage ?? 'No se pudo cambiar el estado.');
    }
  }

  async function handleDuplicate(strategy: Strategy) {
    const result = await strategyService.duplicate(strategy.id, `${strategy.name} (copia)`);
    if (result.isSuccess) {
      addNotification('success', 'Estrategia duplicada');
      refresh();
    } else {
      addNotification('error', result.errorMessage ?? 'No se pudo duplicar la estrategia.');
    }
  }

  async function handleDelete(strategy: Strategy) {
    if (!window.confirm(`¿Eliminar la estrategia "${strategy.name}"? Esta acción no se puede deshacer.`)) {
      return;
    }
    const result = await strategyService.remove(strategy.id);
    if (result.isSuccess) {
      addNotification('success', 'Estrategia eliminada');
      refresh();
    } else {
      addNotification('error', result.errorMessage ?? 'No se pudo eliminar la estrategia.');
    }
  }

  return (
    <div className="h-full overflow-y-auto">
      <div className="max-w-5xl mx-auto px-4 sm:px-6 py-6">
        {/* Header */}
        <div className="flex flex-wrap items-start justify-between gap-4 mb-6">
          <div>
            <h1 className="text-xl font-bold text-neutral-100">Mis Estrategias</h1>
            <p className="text-sm text-neutral-500 mt-1 max-w-xl">
              Una estrategia es el entorno donde conectas bots para automatizar tu trading: un bot
              analiza el mercado, otro avisa y otro ejecuta la orden.
            </p>
          </div>
          <button
            onClick={handleCreate}
            data-tour="new-strategy"
            className="flex items-center gap-2 px-4 py-2 text-sm bg-emerald-500 text-neutral-950 rounded-lg font-semibold hover:bg-emerald-400 transition-colors shrink-0"
          >
            <Plus size={16} />
            Nueva estrategia
          </button>
        </div>

        {loading ? (
          <div className="flex items-center justify-center gap-2 py-20 text-neutral-600 text-sm">
            <Loader2 size={16} className="animate-spin" />
            Cargando estrategias...
          </div>
        ) : strategies.length === 0 ? (
          <EmptyState onCreate={handleCreate} />
        ) : (
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {strategies.map((strategy) => (
              <StrategyCard
                key={strategy.id}
                strategy={strategy}
                onOpen={() => navigate(`/strategy/${strategy.id}`)}
                onToggleStatus={() => toggleStatus(strategy)}
                onDuplicate={() => handleDuplicate(strategy)}
                onDelete={() => handleDelete(strategy)}
              />
            ))}
          </div>
        )}
      </div>
    </div>
  );
}

function EmptyState({ onCreate }: { onCreate: () => void }) {
  return (
    <div className="flex flex-col items-center justify-center text-center gap-4 py-16 border border-dashed border-neutral-800 rounded-2xl">
      <div className="w-14 h-14 rounded-2xl bg-neutral-900 border border-neutral-800 flex items-center justify-center">
        <Workflow size={22} className="text-emerald-400" />
      </div>
      <div>
        <p className="text-neutral-200 font-medium">Crea tu primera estrategia</p>
        <p className="text-sm text-neutral-500 mt-1 max-w-sm">
          En el lienzo arrastras bots y los conectas para definir cuándo analizar, avisar y operar.
        </p>
      </div>
      <button
        onClick={onCreate}
        className="flex items-center gap-2 px-4 py-2 text-sm bg-emerald-500 text-neutral-950 rounded-lg font-semibold hover:bg-emerald-400 transition-colors"
      >
        <Plus size={16} />
        Nueva estrategia
      </button>
    </div>
  );
}

function StrategyCard({
  strategy,
  onOpen,
  onToggleStatus,
  onDuplicate,
  onDelete,
}: {
  strategy: Strategy;
  onOpen: () => void;
  onToggleStatus: () => void;
  onDuplicate: () => void;
  onDelete: () => void;
}) {
  const status = STATUS_META[strategy.status];
  const counts = countBots(strategy);
  const totalBots = strategy.nodes.length;

  return (
    <div className="group flex flex-col rounded-xl border border-neutral-800 bg-neutral-900/50 hover:border-neutral-700 transition-colors">
      {/* Clickable body */}
      <button onClick={onOpen} className="text-left p-4 flex flex-col gap-3 flex-1">
        <div className="flex items-start justify-between gap-2">
          <h3 className="text-sm font-semibold text-neutral-100 truncate">{strategy.name}</h3>
          <span className={`text-[11px] px-2 py-0.5 rounded-full border font-medium shrink-0 ${status.cls}`}>
            {status.label}
          </span>
        </div>

        <div className="flex items-center gap-3">
          {totalBots === 0 ? (
            <span className="text-xs text-neutral-600">Sin bots todavía</span>
          ) : (
            (Object.keys(counts) as BotNodeKind[])
              .filter((kind) => counts[kind] > 0)
              .map((kind) => {
                const { icon: Icon, color } = BOT_META[kind];
                return (
                  <span key={kind} className="flex items-center gap-1 text-xs text-neutral-400">
                    <Icon size={13} className={color} />
                    {counts[kind]}
                  </span>
                );
              })
          )}
        </div>

        <span className="text-[11px] text-neutral-600 mt-auto">
          {totalBots} {totalBots === 1 ? 'bot' : 'bots'} · actualizada {relativeTime(strategy.updatedAt)}
        </span>
      </button>

      {/* Actions */}
      <div className="flex items-center gap-1 px-3 py-2 border-t border-neutral-800">
        <button
          onClick={onToggleStatus}
          title={strategy.status === 'active' ? 'Pausar' : 'Activar'}
          aria-label={strategy.status === 'active' ? 'Pausar estrategia' : 'Activar estrategia'}
          className={`p-1.5 rounded-md transition-colors ${
            strategy.status === 'active'
              ? 'text-amber-400 hover:bg-amber-500/10'
              : 'text-emerald-400 hover:bg-emerald-500/10'
          }`}
        >
          {strategy.status === 'active' ? <Pause size={15} /> : <Play size={15} />}
        </button>
        <button
          onClick={onDuplicate}
          title="Duplicar"
          aria-label="Duplicar estrategia"
          className="p-1.5 rounded-md text-neutral-400 hover:text-neutral-100 hover:bg-neutral-800 transition-colors"
        >
          <Copy size={15} />
        </button>
        <button
          onClick={onDelete}
          title="Eliminar"
          aria-label="Eliminar estrategia"
          className="p-1.5 rounded-md text-neutral-400 hover:text-red-400 hover:bg-red-500/10 transition-colors ml-auto"
        >
          <Trash2 size={15} />
        </button>
      </div>
    </div>
  );
}
