import { useState, useCallback, useRef } from 'react';
import type { DragEvent } from 'react';
import {
  ReactFlow,
  Background,
  BackgroundVariant,
  Controls,
  MiniMap,
  addEdge,
  useNodesState,
  useEdgesState,
  MarkerType,
  type Connection,
  type Node,
  type ReactFlowInstance,
} from '@xyflow/react';
import '@xyflow/react/dist/style.css';
import { Activity, Bell, Zap, Save, Play, Trash2, MousePointerClick } from 'lucide-react';
import { NODE_TYPES, type BotNodeData } from './BotNodes';

type FlowNode = Node<BotNodeData>;

type PaletteType = 'analysisBot' | 'notificationBot' | 'actionBot';

const PALETTE_ITEMS: {
  type: PaletteType;
  label: string;
  description: string;
  icon: typeof Activity;
  colorClasses: string;
  hoverBorder: string;
}[] = [
  {
    type: 'analysisBot',
    label: 'Bot de Análisis',
    description: 'Monitorea condiciones del mercado',
    icon: Activity,
    colorClasses: 'text-blue-400 bg-blue-500/10 border-blue-500/20',
    hoverBorder: 'hover:border-blue-500/25',
  },
  {
    type: 'notificationBot',
    label: 'Bot de Notificación',
    description: 'Envía alertas cuando se activa',
    icon: Bell,
    colorClasses: 'text-amber-400 bg-amber-500/10 border-amber-500/20',
    hoverBorder: 'hover:border-amber-500/25',
  },
  {
    type: 'actionBot',
    label: 'Bot de Acción',
    description: 'Ejecuta una orden automáticamente',
    icon: Zap,
    colorClasses: 'text-emerald-400 bg-emerald-500/10 border-emerald-500/20',
    hoverBorder: 'hover:border-emerald-500/25',
  },
];

const DEFAULT_CONFIGS: Record<PaletteType, BotNodeData['config']> = {
  analysisBot: { symbol: 'BTCUSDT', indicatorType: 'Price', operator: '>', targetValue: 50000 },
  notificationBot: { message: '' },
  actionBot: { action: 'Buy', symbol: 'BTCUSDT', quantity: 0.01 },
};

const LABEL_MAP: Record<PaletteType, string> = {
  analysisBot: 'Bot de Análisis',
  notificationBot: 'Bot de Notificación',
  actionBot: 'Bot de Acción',
};

const SYMBOLS = ['BTCUSDT', 'ETHUSDT', 'SOLUSDT', 'BNBUSDT', 'AAPL', 'TSLA', 'NVDA'];

const inputCls =
  'w-full bg-neutral-800 border border-neutral-700 rounded-lg px-3 py-2 text-sm text-neutral-100 focus:outline-none focus:border-neutral-500';
const labelCls =
  'block text-[11px] font-medium text-neutral-500 mb-1.5 uppercase tracking-wider';

let nodeIdCounter = 0;

export function StrategyBuilderPage() {
  const [nodes, setNodes, onNodesChange] = useNodesState<FlowNode>([]);
  const [edges, setEdges, onEdgesChange] = useEdgesState([]);
  const [selectedNodeId, setSelectedNodeId] = useState<string | null>(null);
  const [strategyName, setStrategyName] = useState('Nueva Estrategia');
  const [rfInstance, setRfInstance] = useState<ReactFlowInstance<FlowNode> | null>(null);
  const wrapperRef = useRef<HTMLDivElement>(null);

  const selectedNode = nodes.find((n) => n.id === selectedNodeId) ?? null;

  const onConnect = useCallback(
    (params: Connection) =>
      // @ts-ignore
          setEdges((eds) =>
        addEdge(
          {
            ...params,
            animated: true,
            // @ts-ignore
            style: { stroke: '#10b981', strokeWidth: 2 },
            markerEnd: { type: MarkerType.ArrowClosed, color: '#10b981' },
          },
          eds,
        ),
      ),
    [setEdges],
  );

  const onDragOver = useCallback((e: DragEvent<HTMLDivElement>) => {
    e.preventDefault();
    e.dataTransfer.dropEffect = 'move';
  }, []);

  const onDrop = useCallback(
    (e: DragEvent<HTMLDivElement>) => {
      e.preventDefault();
      const type = e.dataTransfer.getData('application/reactflow') as PaletteType;
      if (!type || !rfInstance) return;
      const position = rfInstance.screenToFlowPosition({ x: e.clientX, y: e.clientY });
      const id = `node-${++nodeIdCounter}`;
      const newNode: FlowNode = {
        id,
        type,
        position,
        data: { label: LABEL_MAP[type], config: { ...DEFAULT_CONFIGS[type] } },
      };
      setNodes((nds) => [...nds, newNode]);
      setSelectedNodeId(id);
    },
    [rfInstance, setNodes],
  );

  function updateConfig(key: string, value: unknown) {
    if (!selectedNodeId) return;
    setNodes((nds) =>
      nds.map((n) =>
        n.id === selectedNodeId
          ? { ...n, data: { ...n.data, config: { ...n.data.config, [key]: value } } }
          : n,
      ),
    );
  }

  function updateLabel(value: string) {
    if (!selectedNodeId) return;
    setNodes((nds) =>
      nds.map((n) => (n.id === selectedNodeId ? { ...n, data: { ...n.data, label: value } } : n)),
    );
  }

  function deleteSelected() {
    if (!selectedNodeId) return;
    setNodes((nds) => nds.filter((n) => n.id !== selectedNodeId));
    // @ts-ignore
          setEdges((eds) =>
      // @ts-ignore
          eds.filter((e) => e.source !== selectedNodeId && e.target !== selectedNodeId),
    );
    setSelectedNodeId(null);
  }

  return (
    <div className="flex flex-col h-full bg-neutral-950">
      {/* Toolbar */}
      <div className="h-14 shrink-0 border-b border-neutral-800 flex items-center gap-3 px-4">
        <input
          value={strategyName}
          onChange={(e) => setStrategyName(e.target.value)}
          className="bg-transparent border border-neutral-700 rounded-lg px-3 py-1.5 text-sm text-neutral-100 focus:outline-none focus:border-emerald-500/50 w-52"
        />
        <span className="text-xs px-2.5 py-1 rounded-full bg-neutral-800 border border-neutral-700 text-neutral-500 font-medium">
          Borrador
        </span>
        <div className="flex items-center gap-2 ml-auto">
          {selectedNode && (
            <button
              onClick={deleteSelected}
              title="Eliminar nodo seleccionado"
              className="p-2 rounded-lg text-neutral-500 hover:text-red-400 hover:bg-red-500/10 transition-colors"
            >
              <Trash2 size={15} />
            </button>
          )}
          <button className="flex items-center gap-1.5 px-3 py-1.5 text-sm text-neutral-400 border border-neutral-700 rounded-lg hover:border-neutral-600 hover:text-neutral-200 transition-colors">
            <Save size={14} />
            Guardar
          </button>
          <button className="flex items-center gap-1.5 px-3.5 py-1.5 text-sm bg-emerald-500 text-neutral-950 rounded-lg font-semibold hover:bg-emerald-400 transition-colors">
            <Play size={14} />
            Activar
          </button>
        </div>
      </div>

      {/* Body */}
      <div className="flex flex-1 min-h-0">
        {/* Palette */}
        <aside className="w-56 shrink-0 border-r border-neutral-800 bg-neutral-900/30 p-3 flex flex-col gap-1 overflow-y-auto">
          <p className="text-[11px] font-semibold text-neutral-600 uppercase tracking-widest px-1 mb-2">
            Nodos
          </p>
          {PALETTE_ITEMS.map((item) => (
            <PaletteCard key={item.type} item={item} />
          ))}
          <p className="mt-4 px-1 text-[11px] text-neutral-700 leading-relaxed">
            Arrastra un nodo al canvas y conéctalo con otros para crear tu flujo.
          </p>
        </aside>

        {/* Canvas */}
        <div
          ref={wrapperRef}
          className="flex-1 relative"
          onDrop={onDrop}
          onDragOver={onDragOver}
        >
          <ReactFlow
            nodes={nodes}
            edges={edges}
            onNodesChange={onNodesChange}
            onEdgesChange={onEdgesChange}
            onConnect={onConnect}
            onInit={(instance) => setRfInstance(instance as any)}
            nodeTypes={NODE_TYPES}
            onNodeClick={(_, node) => setSelectedNodeId(node.id)}
            onPaneClick={() => setSelectedNodeId(null)}
            colorMode="dark"
            fitView
            proOptions={{ hideAttribution: true }}
          >
            <Background variant={BackgroundVariant.Dots} color="#262626" gap={20} size={1} />
            <Controls />
            <MiniMap
              nodeColor={(n) =>
                n.type === 'analysisBot'
                  ? '#3b82f6'
                  : n.type === 'notificationBot'
                    ? '#f59e0b'
                    : '#10b981'
              }
              maskColor="rgba(10,10,10,0.75)"
            />
          </ReactFlow>

          {nodes.length === 0 && (
            <div className="absolute inset-0 flex flex-col items-center justify-center gap-5 pointer-events-none">
              <div className="flex items-center gap-3 opacity-25">
                <div className="w-16 h-12 rounded-xl border border-blue-500/50 bg-blue-500/5 flex items-center justify-center">
                  <Activity size={18} className="text-blue-400" />
                </div>
                <div className="flex flex-col gap-1">
                  <div className="w-6 h-px bg-neutral-600" />
                  <div className="w-6 h-px bg-neutral-600" />
                </div>
                <div className="w-16 h-12 rounded-xl border border-amber-500/50 bg-amber-500/5 flex items-center justify-center">
                  <Bell size={18} className="text-amber-400" />
                </div>
                <div className="flex flex-col gap-1">
                  <div className="w-6 h-px bg-neutral-600" />
                  <div className="w-6 h-px bg-neutral-600" />
                </div>
                <div className="w-16 h-12 rounded-xl border border-emerald-500/50 bg-emerald-500/5 flex items-center justify-center">
                  <Zap size={18} className="text-emerald-400" />
                </div>
              </div>
              <p className="text-sm text-neutral-600">
                Arrastra nodos desde el panel izquierdo para comenzar
              </p>
            </div>
          )}
        </div>

        {/* Config panel */}
        <aside className="w-72 shrink-0 border-l border-neutral-800 bg-neutral-900/30 overflow-y-auto">
          {selectedNode ? (
            <ConfigPanel
              node={selectedNode}
              onLabelChange={updateLabel}
              onConfigChange={updateConfig}
            />
          ) : (
            <div className="flex flex-col items-center justify-center h-full gap-3 p-6 text-center">
              <div className="w-10 h-10 rounded-xl bg-neutral-800 border border-neutral-700 flex items-center justify-center">
                <MousePointerClick size={16} className="text-neutral-600" />
              </div>
              <div>
                <p className="text-sm font-medium text-neutral-500 mb-1">Sin selección</p>
                <p className="text-xs text-neutral-700 leading-relaxed">
                  Haz clic en un nodo del canvas para ver y editar su configuración.
                </p>
              </div>
            </div>
          )}
        </aside>
      </div>
    </div>
  );
}

function PaletteCard({
  item,
}: {
  item: (typeof PALETTE_ITEMS)[number];
}) {
  const { type, label, description, icon: Icon, colorClasses, hoverBorder } = item;

  function onDragStart(e: DragEvent<HTMLDivElement>) {
    e.dataTransfer.setData('application/reactflow', type);
    e.dataTransfer.effectAllowed = 'move';
  }

  return (
    <div
      draggable
      onDragStart={onDragStart}
      className={`flex items-start gap-3 p-3 rounded-xl border border-neutral-800 bg-neutral-900/50 cursor-grab active:cursor-grabbing hover:bg-neutral-800/50 transition-all select-none ${hoverBorder}`}
    >
      <div
        className={`w-7 h-7 rounded-lg border flex items-center justify-center shrink-0 ${colorClasses}`}
      >
        <Icon size={13} />
      </div>
      <div>
        <p className="text-xs font-semibold text-neutral-200">{label}</p>
        <p className="text-[11px] text-neutral-600 mt-0.5 leading-relaxed">{description}</p>
      </div>
    </div>
  );
}

function ConfigPanel({
  node,
  onLabelChange,
  onConfigChange,
}: {
  node: FlowNode;
  onLabelChange: (v: string) => void;
  onConfigChange: (key: string, value: unknown) => void;
}) {
  const { type, data } = node;
  const { label, config } = data;

  const typeIcon = { analysisBot: Activity, notificationBot: Bell, actionBot: Zap }[type ?? ''];
  const typeColor = {
    analysisBot: 'text-blue-400',
    notificationBot: 'text-amber-400',
    actionBot: 'text-emerald-400',
  }[type ?? ''];
  const typeTitle = {
    analysisBot: 'Bot de Análisis',
    notificationBot: 'Bot de Notificación',
    actionBot: 'Bot de Acción',
  }[type ?? ''];

  const TypeIcon = typeIcon;

  return (
    <div className="p-4 flex flex-col gap-4">
      {/* Header */}
      <div className="flex items-center gap-2 pb-3 border-b border-neutral-800">
        {TypeIcon && <TypeIcon size={14} className={typeColor} />}
        <span className="text-sm font-semibold text-neutral-200">{typeTitle ?? 'Nodo'}</span>
      </div>

      {/* Name */}
      <div>
        <label className={labelCls}>Nombre</label>
        <input
          value={label}
          onChange={(e) => onLabelChange(e.target.value)}
          className={inputCls}
          placeholder="Nombre del bot"
        />
      </div>

      {/* Analysis Bot fields */}
      {type === 'analysisBot' && (
        <>
          <div>
            <label className={labelCls}>Símbolo</label>
            <select
              value={(config.symbol as string) ?? 'BTCUSDT'}
              onChange={(e) => onConfigChange('symbol', e.target.value)}
              className={`${inputCls} cursor-pointer`}
            >
              {SYMBOLS.map((s) => (
                <option key={s} value={s}>
                  {s}
                </option>
              ))}
            </select>
          </div>
          <div>
            <label className={labelCls}>Indicador</label>
            <select
              value={(config.indicatorType as string) ?? 'Price'}
              onChange={(e) => onConfigChange('indicatorType', e.target.value)}
              className={`${inputCls} cursor-pointer`}
            >
              <option value="Price">Precio</option>
              <option value="RSI">RSI</option>
              <option value="MA">Media Móvil (MA)</option>
            </select>
          </div>
          <div className="grid grid-cols-2 gap-2">
            <div>
              <label className={labelCls}>Condición</label>
              <select
                value={(config.operator as string) ?? '>'}
                onChange={(e) => onConfigChange('operator', e.target.value)}
                className={`${inputCls} cursor-pointer`}
              >
                {(['>', '<', '>=', '<=', '=='] as const).map((op) => (
                  <option key={op} value={op}>
                    {op}
                  </option>
                ))}
              </select>
            </div>
            <div>
              <label className={labelCls}>Valor objetivo</label>
              <input
                type="number"
                value={(config.targetValue as number) ?? 0}
                onChange={(e) => onConfigChange('targetValue', parseFloat(e.target.value))}
                className={inputCls}
              />
            </div>
          </div>
        </>
      )}

      {/* Notification Bot fields */}
      {type === 'notificationBot' && (
        <div>
          <label className={labelCls}>Mensaje de alerta</label>
          <textarea
            value={(config.message as string) ?? ''}
            onChange={(e) => onConfigChange('message', e.target.value)}
            placeholder="Condición detectada en {symbol}..."
            rows={3}
            className={`${inputCls} resize-none`}
          />
          <p className="mt-1.5 text-[11px] text-neutral-700">
            Usa {'{symbol}'} como variable dinámica.
          </p>
        </div>
      )}

      {/* Action Bot fields */}
      {type === 'actionBot' && (
        <>
          <div>
            <label className={labelCls}>Acción</label>
            <div className="grid grid-cols-2 gap-2">
              {(['Buy', 'Sell'] as const).map((a) => (
                <button
                  key={a}
                  onClick={() => onConfigChange('action', a)}
                  className={`py-2 rounded-lg text-sm font-semibold border transition-colors ${
                    config.action === a
                      ? a === 'Buy'
                        ? 'bg-emerald-500/15 border-emerald-500/40 text-emerald-400'
                        : 'bg-red-500/15 border-red-500/40 text-red-400'
                      : 'bg-neutral-800 border-neutral-700 text-neutral-500 hover:border-neutral-600'
                  }`}
                >
                  {a === 'Buy' ? 'Comprar' : 'Vender'}
                </button>
              ))}
            </div>
          </div>
          <div>
            <label className={labelCls}>Símbolo</label>
            <select
              value={(config.symbol as string) ?? 'BTCUSDT'}
              onChange={(e) => onConfigChange('symbol', e.target.value)}
              className={`${inputCls} cursor-pointer`}
            >
              {SYMBOLS.map((s) => (
                <option key={s} value={s}>
                  {s}
                </option>
              ))}
            </select>
          </div>
          <div>
            <label className={labelCls}>Cantidad</label>
            <input
              type="number"
              value={(config.quantity as number) ?? 0.01}
              onChange={(e) => onConfigChange('quantity', parseFloat(e.target.value))}
              step={0.01}
              min={0}
              className={inputCls}
            />
          </div>
        </>
      )}
    </div>
  );
}
