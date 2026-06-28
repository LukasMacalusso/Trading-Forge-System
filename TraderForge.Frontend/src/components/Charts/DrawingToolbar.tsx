import {
  MousePointer2,
  Slash,
  Minus,
  MoveUpRight,
  Square,
  Circle,
  ArrowUpRight,
  Brush,
  Type,
  AlignJustify,
  Ruler,
  Magnet,
  Eye,
  EyeOff,
  Trash2,
} from 'lucide-react';
import type { LucideIcon } from 'lucide-react';
import type { DrawingToolId } from '@models/Drawing';
import type { DrawingToolsApi } from './useChartDrawings';

interface ToolDef {
  id: DrawingToolId;
  icon: LucideIcon;
  label: string;
  rotate?: boolean;
}

const TOOL_GROUPS: ToolDef[][] = [
  [{ id: 'cursor', icon: MousePointer2, label: 'Cursor' }],
  [
    { id: 'trendline', icon: Slash, label: 'Línea de tendencia' },
    { id: 'horizontal', icon: Minus, label: 'Línea horizontal' },
    { id: 'vertical', icon: Minus, label: 'Línea vertical', rotate: true },
    { id: 'ray', icon: MoveUpRight, label: 'Rayo' },
  ],
  [
    { id: 'rectangle', icon: Square, label: 'Rectángulo' },
    { id: 'ellipse', icon: Circle, label: 'Elipse' },
    { id: 'arrow', icon: ArrowUpRight, label: 'Flecha' },
    { id: 'brush', icon: Brush, label: 'Pincel' },
  ],
  [
    { id: 'fib', icon: AlignJustify, label: 'Retroceso de Fibonacci' },
    { id: 'measure', icon: Ruler, label: 'Medir' },
    { id: 'text', icon: Type, label: 'Texto' },
  ],
];

const buttonCls =
  'w-8 h-8 flex items-center justify-center rounded-md transition-colors';

export function DrawingToolbar({
  activeTool,
  setActiveTool,
  magnet,
  toggleMagnet,
  hidden,
  toggleHidden,
  hasDrawings,
  clearAll,
}: DrawingToolsApi) {
  return (
    <div className="absolute top-2 left-2 z-20 flex flex-col items-center gap-1 p-1 rounded-xl border border-neutral-800 bg-neutral-900/90 backdrop-blur-sm shadow-xl">
      {TOOL_GROUPS.map((group, i) => (
        <div key={i} className="flex flex-col items-center gap-1">
          {i > 0 && <span className="my-0.5 h-px w-5 bg-neutral-800" />}
          {group.map(({ id, icon: Icon, label, rotate }) => {
            const active = activeTool === id;
            return (
              <button
                key={id}
                onClick={() => setActiveTool(id)}
                aria-label={label}
                aria-pressed={active}
                title={label}
                className={`${buttonCls} ${
                  active
                    ? 'bg-amber-500/15 text-amber-400'
                    : 'text-neutral-400 hover:text-neutral-100 hover:bg-neutral-800'
                }`}
              >
                <Icon size={16} className={rotate ? 'rotate-90' : undefined} />
              </button>
            );
          })}
        </div>
      ))}

      <span className="my-0.5 h-px w-5 bg-neutral-800" />

      <button
        onClick={toggleMagnet}
        aria-label="Imán: ajustar a precios"
        aria-pressed={magnet}
        title="Imán (ajustar a OHLC)"
        className={`${buttonCls} ${
          magnet ? 'bg-amber-500/15 text-amber-400' : 'text-neutral-400 hover:text-neutral-100 hover:bg-neutral-800'
        }`}
      >
        <Magnet size={16} />
      </button>

      <button
        onClick={toggleHidden}
        aria-label={hidden ? 'Mostrar dibujos' : 'Ocultar dibujos'}
        aria-pressed={hidden}
        title={hidden ? 'Mostrar dibujos' : 'Ocultar dibujos'}
        className={`${buttonCls} text-neutral-400 hover:text-neutral-100 hover:bg-neutral-800`}
      >
        {hidden ? <EyeOff size={16} /> : <Eye size={16} />}
      </button>

      <button
        onClick={clearAll}
        disabled={!hasDrawings}
        aria-label="Borrar todos los dibujos"
        title="Borrar todo"
        className={`${buttonCls} text-neutral-400 hover:text-red-400 hover:bg-red-500/10 disabled:opacity-30 disabled:cursor-not-allowed disabled:hover:bg-transparent disabled:hover:text-neutral-400`}
      >
        <Trash2 size={16} />
      </button>
    </div>
  );
}
