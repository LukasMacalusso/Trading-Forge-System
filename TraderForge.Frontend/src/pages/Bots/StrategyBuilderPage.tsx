import { useState, useCallback, useRef, useEffect } from 'react';
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
  type Edge,
  type ReactFlowInstance,
} from '@xyflow/react';
import '@xyflow/react/dist/style.css';
import {
  Activity,
  Bell,
  Zap,
  Save,
  Play,
  Trash2,
  MousePointerClick,
  PanelLeft,
  SlidersHorizontal,
  X,
} from 'lucide-react';
import { NODE_TYPES } from './BotNodes';
import type {
  BotNodeData,
  BotNodeKind,
  AnalysisBotConfig,
  NotificationBotConfig,
  ActionBotConfig,
} from '@models/BotFlow';
import {
  StrategyRepository,
  type StrategyNode,
  type WorkspaceStatus,
} from '@utils/StrategyRepository';
import { useNotificationStore } from '@store/notificationStore';

const PALETTE_ITEMS: {
  kind: BotNodeKind;
  label: string;
  description: string;
  icon: typeof Activity;
  colorClasses: string;
  hoverBorder: string;
}[] = [
  {
    kind: 'analysisBot',
    label: 'Bot de Análisis',
    description: 'Monitorea condiciones del mercado',
    icon: Activity,
    colorClasses: 'text-blue-400 bg-blue-500/10 border-blue-500/20',
    hoverBorder: 'hover:border-blue-500/25',
  },
  {
    kind: 'notificationBot',
    label: 'Bot de Notificación',
    description: 'Envía alertas cuando se activa',
    icon: Bell,
    colorClasses: 'text-amber-400 bg-amber-500/10 border-amber-500/20',
    hoverBorder: 'hover:border-amber-500/25',
  },
  {
    kind: 'actionBot',
    label: 'Bot de Acción',
    description: 'Ejecuta una orden automáticamente',
    icon: Zap,
    colorClasses: 'text-emerald-400 bg-emerald-500/10 border-emerald-500/20',
    hoverBorder: 'hover:border-emerald-500/25',
  },
];

const SYMBOLS = ['BTCUSDT', 'ETHUSDT', 'SOLUSDT', 'BNBUSDT', 'AAPL', 'TSLA', 'NVDA'];

const inputCls =
  'w-full bg-neutral-800 border border-neutral-700 rounded-lg px-3 py-2 text-sm text-neutral-100 focus:outline-none focus:border-emerald-500/50 focus:ring-1 focus:ring-emerald-500/30 transition-colors';
const labelCls =
  'block text-[11px] font-medium text-neutral-500 mb-1.5 uppercase tracking-wider';

function createNodeData(kind: BotNodeKind): BotNodeData {
  switch (kind) {
    case 'analysisBot':
      return {
        kind,
        label: 'Bot de Análisis',
        config: { symbol: 'BTCUSDT', indicatorType: 'Price', operator: '>', targetValue: 50000 },
      };
    case 'notificationBot':
      return { kind, label: 'Bot de Notificación', config: { message: '' } };
    case 'actionBot':
      return {
        kind,
        label: 'Bot de Acción',
        config: { action: 'Buy', symbol: 'BTCUSDT', quantity: 0.01 },
      };
  }
}

/** Keeps generated ids unique across the session and restored drafts. */
let nodeIdCounter = 0;
function nextNodeId(): string {
  return `node-${++nodeIdCounter}`;
}

export function StrategyBuilderPage() {
  const [nodes, setNodes, onNodesChange] = useNodesState<StrategyNode>([]);
  const [edges, setEdges, onEdgesChange] = useEdgesState<Edge>([]);
  const [selectedNodeId, setSelectedNodeId] = useState<string | null>(null);
  const [strategyName, setStrategyName] = useState('Nueva Estrategia');
  const [status, setStatus] = useState<WorkspaceStatus>('draft');
  const [rfInstance, setRfInstance] = useState<ReactFlowInstance<StrategyNode> | null>(null);
  const [paletteOpen, setPaletteOpen] = useState(false);
  const [configOpen, setConfigOpen] = useState(false);
  const wrapperRef = useRef<HTMLDivElement>(null);

  const addNotification = useNotificationStore((s) => s.addNotification);
  const selectedNode = nodes.find((n) => n.id === selectedNodeId) ?? null;

  // Restore the locally saved draft on first mount (mock persistence — see TFS-23).
  useEffect(() => {
    const saved = StrategyRepository.load();
    if (!saved) return;
    setNodes(saved.nodes);
    setEdges(saved.edges);
    setStrategyName(saved.name);
    setStatus(saved.status);
    nodeIdCounter = saved.nodes.reduce((max, n) => {
      const num = Number(n.id.replace('node-', ''));
      return Number.isFinite(num) && num > max ? num : max;
    }, nodeIdCounter);
  }, [setNodes, setEdges]);

  const onConnect = useCallback(
    (params: Connection) =>
      setEdges((eds) =>
        addEdge(
          {
            ...params,
            animated: true,
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
      const kind = e.dataTransfer.getData('application/reactflow') as BotNodeKind;
      if (!kind || !rfInstance) return;
      const position = rfInstance.screenToFlowPosition({ x: e.clientX, y: e.clientY });
      const id = nextNodeId();
      setNodes((nds) => [...nds, { id, type: kind, position, data: createNodeData(kind) }]);
      setSelectedNodeId(id);
    },
    [rfInstance, setNodes],
  );

  /** Merges a partial config patch into the selected node. */
  function patchConfig(
    patch: Partial<AnalysisBotConfig & NotificationBotConfig & ActionBotConfig>,
  ) {
    if (!selectedNodeId) return;
    setNodes((nds) =>
      nds.map((n) =>
        n.id === selectedNodeId
          ? // Discriminant (`kind`) is preserved; only known config fields change.
            ({ ...n, data: { ...n.data, config: { ...n.data.config, ...patch } } } as StrategyNode)
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
    setEdges((eds) => eds.filter((e) => e.source !== selectedNodeId && e.target !== selectedNodeId));
    setSelectedNodeId(null);
  }

  function selectNode(id: string) {
    setSelectedNodeId(id);
    setConfigOpen(true);
  }

  function clearSelection() {
    setSelectedNodeId(null);
    setConfigOpen(false);
  }

  function persist(nextStatus: WorkspaceStatus) {
    StrategyRepository.save({
      name: strategyName.trim() || 'Estrategia sin nombre',
      status: nextStatus,
      nodes,
      edges,
      savedAt: new Date().toISOString(),
    });
    setStatus(nextStatus);
  }

  function handleSave() {
    persist('draft');
    addNotification('success', 'Estrategia guardada');
  }

  function handleActivate() {
    if (nodes.length === 0) {
      addNotification('warning', 'Agrega al menos un nodo antes de activar la estrategia');
      return;
    }
    persist('active');
    addNotification('success', `Estrategia "${strategyName.trim() || 'sin nombre'}" activada`);
  }

  return (
    <div className="flex flex-col h-full bg-neutral-950">
      {/* Toolbar */}
      <div className="min-h-14 shrink-0 border-b border-neutral-800 flex flex-wrap items-center gap-2 sm:gap-3 px-3 sm:px-4 py-2">
        <button
          onClick={() => setPaletteOpen((v) => !v)}
          aria-label="Mostrar u ocultar panel de nodos"
          className="lg:hidden p-2 rounded-lg text-neutral-400 hover:text-neutral-200 hover:bg-neutral-800 transition-colors"
        >
          <PanelLeft size={16} />
        </button>

        <input
          value={strategyName}
          onChange={(e) => setStrategyName(e.target.value)}
          aria-label="Nombre de la estrategia"
          className="bg-transparent border border-neutral-700 rounded-lg px-3 py-1.5 text-sm text-neutral-100 focus:outline-none focus:border-emerald-500/50 w-40 sm:w-52"
        />
        <span
          className={`text-xs px-2.5 py-1 rounded-full border font-medium ${
            status === 'active'
              ? 'bg-emerald-500/10 border-emerald-500/30 text-emerald-400'
              : 'bg-neutral-800 border-neutral-700 text-neutral-500'
          }`}
        >
          {status === 'active' ? 'Activa' : 'Borrador'}
        </span>

        <div className="flex items-center gap-2 ml-auto">
          {selectedNode && (
            <button
              onClick={deleteSelected}
              aria-label="Eliminar nodo seleccionado"
              title="Eliminar nodo seleccionado"
              className="p-2 rounded-lg text-neutral-500 hover:text-red-400 hover:bg-red-500/10 transition-colors"
            >
              <Trash2 size={15} />
            </button>
          )}
          <button
            onClick={handleSave}
            className="flex items-center gap-1.5 px-3 py-1.5 text-sm text-neutral-400 border border-neutral-700 rounded-lg hover:border-neutral-600 hover:text-neutral-200 transition-colors"
          >
            <Save size={14} />
            <span className="hidden sm:inline">Guardar</span>
          </button>
          <button
            onClick={handleActivate}
            disabled={nodes.length === 0}
            className="flex items-center gap-1.5 px-3.5 py-1.5 text-sm bg-emerald-500 text-neutral-950 rounded-lg font-semibold hover:bg-emerald-400 transition-colors disabled:opacity-40 disabled:cursor-not-allowed"
          >
            <Play size={14} />
            <span className="hidden sm:inline">Activar</span>
          </button>
          <button
            onClick={() => setConfigOpen((v) => !v)}
            aria-label="Mostrar u ocultar panel de configuración"
            className="lg:hidden p-2 rounded-lg text-neutral-400 hover:text-neutral-200 hover:bg-neutral-800 transition-colors"
          >
            <SlidersHorizontal size={16} />
          </button>
        </div>
      </div>

      {/* Body */}
      <div className="flex flex-1 min-h-0 relative">
        {/* Palette */}
        <aside
          className={`w-56 shrink-0 border-r border-neutral-800 bg-neutral-900 lg:bg-neutral-900/30 p-3 flex flex-col gap-1 overflow-y-auto absolute inset-y-0 left-0 z-30 transition-transform duration-200 lg:static lg:translate-x-0 ${
            paletteOpen ? 'translate-x-0' : '-translate-x-full'
          }`}
        >
          <div className="flex items-center justify-between mb-2">
            <p className="text-[11px] font-semibold text-neutral-600 uppercase tracking-widest px-1">
              Nodos
            </p>
            <button
              onClick={() => setPaletteOpen(false)}
              aria-label="Cerrar panel de nodos"
              className="lg:hidden p-1 text-neutral-500 hover:text-neutral-300"
            >
              <X size={14} />
            </button>
          </div>
          {PALETTE_ITEMS.map((item) => (
            <PaletteCard key={item.kind} item={item} />
          ))}
          <p className="mt-4 px-1 text-[11px] text-neutral-700 leading-relaxed">
            Arrastra un nodo al canvas y conéctalo con otros para crear tu flujo.
          </p>
        </aside>

        {/* Canvas */}
        <div ref={wrapperRef} className="flex-1 relative" onDrop={onDrop} onDragOver={onDragOver}>
          <ReactFlow
            nodes={nodes}
            edges={edges}
            onNodesChange={onNodesChange}
            onEdgesChange={onEdgesChange}
            onConnect={onConnect}
            onInit={setRfInstance}
            nodeTypes={NODE_TYPES}
            onNodeClick={(_, node) => selectNode(node.id)}
            onPaneClick={clearSelection}
            onNodesDelete={(deleted) => {
              if (deleted.some((n) => n.id === selectedNodeId)) clearSelection();
            }}
            deleteKeyCode={['Backspace', 'Delete']}
            colorMode="dark"
            fitView
            proOptions={{ hideAttribution: true }}
          >
            <Background variant={BackgroundVariant.Dots} color="#262626" gap={20} size={1} />
            <Controls />
            <MiniMap
              className="!hidden sm:!block"
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
            <div className="absolute inset-0 flex flex-col items-center justify-center gap-5 pointer-events-none px-4 text-center">
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
                <span className="hidden lg:inline">
                  Arrastra nodos desde el panel izquierdo para comenzar
                </span>
                <span className="lg:hidden">
                  Abre el panel de nodos y arrastra uno al canvas para comenzar
                </span>
              </p>
            </div>
          )}
        </div>

        {/* Config panel */}
        <aside
          className={`w-72 shrink-0 border-l border-neutral-800 bg-neutral-900 lg:bg-neutral-900/30 overflow-y-auto absolute inset-y-0 right-0 z-30 transition-transform duration-200 lg:static lg:translate-x-0 ${
            configOpen ? 'translate-x-0' : 'translate-x-full'
          }`}
        >
          {selectedNode ? (
            <ConfigPanel
              node={selectedNode}
              onClose={() => setConfigOpen(false)}
              onLabelChange={updateLabel}
              onConfigChange={patchConfig}
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

        {/* Mobile backdrop */}
        {(paletteOpen || configOpen) && (
          <button
            aria-label="Cerrar paneles"
            onClick={() => {
              setPaletteOpen(false);
              setConfigOpen(false);
            }}
            className="lg:hidden absolute inset-0 bg-black/40 z-20"
          />
        )}
      </div>
    </div>
  );
}

function PaletteCard({ item }: { item: (typeof PALETTE_ITEMS)[number] }) {
  const { kind, label, description, icon: Icon, colorClasses, hoverBorder } = item;

  function onDragStart(e: DragEvent<HTMLDivElement>) {
    e.dataTransfer.setData('application/reactflow', kind);
    e.dataTransfer.effectAllowed = 'move';
  }

  return (
    <div
      draggable
      onDragStart={onDragStart}
      role="button"
      aria-label={`Arrastrar ${label} al canvas`}
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

const PANEL_META: Record<BotNodeKind, { icon: typeof Activity; color: string; title: string }> = {
  analysisBot: { icon: Activity, color: 'text-blue-400', title: 'Bot de Análisis' },
  notificationBot: { icon: Bell, color: 'text-amber-400', title: 'Bot de Notificación' },
  actionBot: { icon: Zap, color: 'text-emerald-400', title: 'Bot de Acción' },
};

function ConfigPanel({
  node,
  onClose,
  onLabelChange,
  onConfigChange,
}: {
  node: StrategyNode;
  onClose: () => void;
  onLabelChange: (v: string) => void;
  onConfigChange: (patch: Partial<AnalysisBotConfig & NotificationBotConfig & ActionBotConfig>) => void;
}) {
  const { data } = node;
  const meta = PANEL_META[data.kind];
  const MetaIcon = meta.icon;

  return (
    <div className="p-4 flex flex-col gap-4">
      {/* Header */}
      <div className="flex items-center gap-2 pb-3 border-b border-neutral-800">
        <MetaIcon size={14} className={meta.color} />
        <span className="text-sm font-semibold text-neutral-200">{meta.title}</span>
        <button
          onClick={onClose}
          aria-label="Cerrar panel de configuración"
          className="lg:hidden ml-auto p-1 text-neutral-500 hover:text-neutral-300"
        >
          <X size={14} />
        </button>
      </div>

      {/* Name */}
      <div>
        <label className={labelCls}>Nombre</label>
        <input
          value={data.label}
          onChange={(e) => onLabelChange(e.target.value)}
          className={inputCls}
          placeholder="Nombre del bot"
        />
      </div>

      {data.kind === 'analysisBot' && (
        <AnalysisFields config={data.config} onChange={onConfigChange} />
      )}
      {data.kind === 'notificationBot' && (
        <NotificationFields config={data.config} onChange={onConfigChange} />
      )}
      {data.kind === 'actionBot' && <ActionFields config={data.config} onChange={onConfigChange} />}
    </div>
  );
}

function AnalysisFields({
  config,
  onChange,
}: {
  config: AnalysisBotConfig;
  onChange: (patch: Partial<AnalysisBotConfig>) => void;
}) {
  return (
    <>
      <div>
        <label className={labelCls}>Símbolo</label>
        <select
          value={config.symbol}
          onChange={(e) => onChange({ symbol: e.target.value })}
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
          value={config.indicatorType}
          onChange={(e) => onChange({ indicatorType: e.target.value as AnalysisBotConfig['indicatorType'] })}
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
            value={config.operator}
            onChange={(e) => onChange({ operator: e.target.value as AnalysisBotConfig['operator'] })}
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
            value={config.targetValue}
            onChange={(e) => onChange({ targetValue: parseFloat(e.target.value) || 0 })}
            className={inputCls}
          />
        </div>
      </div>
    </>
  );
}

function NotificationFields({
  config,
  onChange,
}: {
  config: NotificationBotConfig;
  onChange: (patch: Partial<NotificationBotConfig>) => void;
}) {
  return (
    <div>
      <label className={labelCls}>Mensaje de alerta</label>
      <textarea
        value={config.message}
        onChange={(e) => onChange({ message: e.target.value })}
        placeholder="Condición detectada en {symbol}..."
        rows={3}
        className={`${inputCls} resize-none`}
      />
      <p className="mt-1.5 text-[11px] text-neutral-700">
        Usa {'{symbol}'} como variable dinámica.
      </p>
    </div>
  );
}

function ActionFields({
  config,
  onChange,
}: {
  config: ActionBotConfig;
  onChange: (patch: Partial<ActionBotConfig>) => void;
}) {
  return (
    <>
      <div>
        <label className={labelCls}>Acción</label>
        <div className="grid grid-cols-2 gap-2">
          {(['Buy', 'Sell'] as const).map((a) => (
            <button
              key={a}
              onClick={() => onChange({ action: a })}
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
          value={config.symbol}
          onChange={(e) => onChange({ symbol: e.target.value })}
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
          value={config.quantity}
          onChange={(e) => onChange({ quantity: parseFloat(e.target.value) || 0 })}
          step={0.01}
          min={0}
          className={inputCls}
        />
      </div>
    </>
  );
}
