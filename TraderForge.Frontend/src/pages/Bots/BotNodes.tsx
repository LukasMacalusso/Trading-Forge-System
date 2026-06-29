import { memo } from 'react';
import { Handle, Position } from '@xyflow/react';
import type { NodeProps, Node } from '@xyflow/react';
import { Activity, Bell, Zap } from 'lucide-react';
import type { BotNodeData } from '@models/BotFlow';

export type { BotNodeData };

type NodeOf<K extends BotNodeData['kind']> = Node<Extract<BotNodeData, { kind: K }>>;

const handleCls =
  '!w-3 !h-3 !bg-neutral-800 !border-2 !border-neutral-600 hover:!border-emerald-500 !transition-colors';

export const AnalysisBotNode = memo(({ data, selected }: NodeProps<NodeOf<'analysisBot'>>) => {
  const { label, config } = data;
  const summary = `${config.indicatorType} ${config.symbol} ${config.operator} ${config.targetValue}`;

  return (
    <div
      className={`w-52 rounded-xl border bg-neutral-900 shadow-xl overflow-hidden transition-colors ${
        selected ? 'border-blue-500/50 shadow-blue-500/10' : 'border-neutral-700/60'
      }`}
    >
      <div className="flex items-center gap-2 px-3 py-2 bg-blue-500/10 border-b border-blue-500/15">
        <div className="w-5 h-5 rounded-md bg-blue-500/20 flex items-center justify-center">
          <Activity size={11} className="text-blue-400" />
        </div>
        <span className="text-xs font-semibold text-blue-300">Bot de Análisis</span>
      </div>
      <div className="px-3 py-2.5">
        <p className="text-sm font-medium text-neutral-100 mb-1 truncate">{label}</p>
        <p className="text-xs text-neutral-500 truncate">{summary}</p>
      </div>
      <Handle type="source" position={Position.Bottom} className={handleCls} />
    </div>
  );
});
AnalysisBotNode.displayName = 'AnalysisBotNode';

export const NotificationBotNode = memo(({ data, selected }: NodeProps<NodeOf<'notificationBot'>>) => {
  const { label, config } = data;

  return (
    <div
      className={`w-52 rounded-xl border bg-neutral-900 shadow-xl overflow-hidden transition-colors ${
        selected ? 'border-amber-500/50 shadow-amber-500/10' : 'border-neutral-700/60'
      }`}
    >
      <Handle type="target" position={Position.Top} className={handleCls} />
      <div className="flex items-center gap-2 px-3 py-2 bg-amber-500/10 border-b border-amber-500/15">
        <div className="w-5 h-5 rounded-md bg-amber-500/20 flex items-center justify-center">
          <Bell size={11} className="text-amber-400" />
        </div>
        <span className="text-xs font-semibold text-amber-300">Bot de Notificación</span>
      </div>
      <div className="px-3 py-2.5">
        <p className="text-sm font-medium text-neutral-100 mb-1 truncate">{label}</p>
        <p className="text-xs text-neutral-500 truncate">
          {config.message || 'Sin mensaje configurado'}
        </p>
      </div>
      <Handle type="source" position={Position.Bottom} className={handleCls} />
    </div>
  );
});
NotificationBotNode.displayName = 'NotificationBotNode';

export const ActionBotNode = memo(({ data, selected }: NodeProps<NodeOf<'actionBot'>>) => {
  const { label, config } = data;
  const summary = `${config.action === 'Buy' ? 'Comprar' : 'Vender'} ${config.quantity} ${config.symbol}`;

  return (
    <div
      className={`w-52 rounded-xl border bg-neutral-900 shadow-xl overflow-hidden transition-colors ${
        selected ? 'border-emerald-500/50 shadow-emerald-500/10' : 'border-neutral-700/60'
      }`}
    >
      <Handle type="target" position={Position.Top} className={handleCls} />
      <div className="flex items-center gap-2 px-3 py-2 bg-emerald-500/10 border-b border-emerald-500/15">
        <div className="w-5 h-5 rounded-md bg-emerald-500/20 flex items-center justify-center">
          <Zap size={11} className="text-emerald-400" />
        </div>
        <span className="text-xs font-semibold text-emerald-300">Bot de Acción</span>
      </div>
      <div className="px-3 py-2.5">
        <p className="text-sm font-medium text-neutral-100 mb-1 truncate">{label}</p>
        <p className="text-xs text-neutral-500 truncate">{summary}</p>
      </div>
    </div>
  );
});
ActionBotNode.displayName = 'ActionBotNode';

export const NODE_TYPES = {
  analysisBot: AnalysisBotNode,
  notificationBot: NotificationBotNode,
  actionBot: ActionBotNode,
};
