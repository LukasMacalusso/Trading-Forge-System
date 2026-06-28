import { useCallback, useEffect, useRef, useState } from 'react';
import type { RefObject } from 'react';
import type { IChartApi, ISeriesApi } from 'lightweight-charts';
import type { CandlestickBar } from '@models/Asset';
import {
  type Drawing,
  type DrawingPoint,
  type DrawingToolId,
  SINGLE_POINT_TOOLS,
} from '@models/Drawing';
import { DrawingRepository } from '@utils/DrawingRepository';
import { DrawingPrimitive, type DrawingRenderState } from './drawingPrimitive';

const DEFAULT_COLOR = '#3b82f6';
const HIT_THRESHOLD = 6;

interface Params {
  chartRef: RefObject<IChartApi | null>;
  seriesRef: RefObject<ISeriesApi<'Candlestick'> | null>;
  containerRef: RefObject<HTMLDivElement | null>;
  candlesRef: RefObject<CandlestickBar[]>;
  ready: boolean;
  symbol: string;
}

export interface DrawingToolsApi {
  activeTool: DrawingToolId;
  setActiveTool: (tool: DrawingToolId) => void;
  magnet: boolean;
  toggleMagnet: () => void;
  hidden: boolean;
  toggleHidden: () => void;
  hasDrawings: boolean;
  clearAll: () => void;
  undo: () => void;
  redo: () => void;
  canUndo: boolean;
  canRedo: boolean;
}

const cloneDrawings = (list: Drawing[]): Drawing[] =>
  list.map((d) => ({ ...d, points: d.points.map((p) => ({ ...p })) }));

function distanceToSegment(px: number, py: number, ax: number, ay: number, bx: number, by: number): number {
  const dx = bx - ax;
  const dy = by - ay;
  const lenSq = dx * dx + dy * dy;
  if (lenSq === 0) return Math.hypot(px - ax, py - ay);
  let t = ((px - ax) * dx + (py - ay) * dy) / lenSq;
  t = Math.max(0, Math.min(1, t));
  return Math.hypot(px - (ax + t * dx), py - (ay + t * dy));
}

export function useChartDrawings({
  chartRef,
  seriesRef,
  containerRef,
  candlesRef,
  ready,
  symbol,
}: Params): DrawingToolsApi {
  const stateRef = useRef<DrawingRenderState>({
    drawings: [],
    draft: null,
    selectedId: null,
    hidden: false,
  });
  const primitiveRef = useRef<DrawingPrimitive | null>(null);
  const activeToolRef = useRef<DrawingToolId>('cursor');
  const magnetRef = useRef(false);
  const symbolRef = useRef(symbol);
  const pastRef = useRef<Drawing[][]>([]);
  const futureRef = useRef<Drawing[][]>([]);
  const moveRef = useRef<{
    id: string;
    startLogical: number;
    startPrice: number;
    origin: DrawingPoint[];
    moved: boolean;
  } | null>(null);

  const [activeTool, setActiveToolState] = useState<DrawingToolId>('cursor');
  const [magnet, setMagnet] = useState(false);
  const [hidden, setHidden] = useState(false);
  const [hasDrawings, setHasDrawings] = useState(false);
  const [canUndo, setCanUndo] = useState(false);
  const [canRedo, setCanRedo] = useState(false);

  const repaint = useCallback(() => primitiveRef.current?.requestUpdate(), []);

  const persist = useCallback(() => {
    DrawingRepository.save(symbolRef.current, stateRef.current.drawings);
    setHasDrawings(stateRef.current.drawings.length > 0);
  }, []);

  const updateHistoryFlags = useCallback(() => {
    setCanUndo(pastRef.current.length > 0);
    setCanRedo(futureRef.current.length > 0);
  }, []);

  /** Pushes the current drawings onto the undo stack before a mutation. */
  const snapshot = useCallback(() => {
    pastRef.current.push(cloneDrawings(stateRef.current.drawings));
    if (pastRef.current.length > 60) pastRef.current.shift();
    futureRef.current = [];
    updateHistoryFlags();
  }, [updateHistoryFlags]);

  const undo = useCallback(() => {
    const prev = pastRef.current.pop();
    if (!prev) return;
    futureRef.current.push(cloneDrawings(stateRef.current.drawings));
    stateRef.current.drawings = prev;
    stateRef.current.selectedId = null;
    persist();
    repaint();
    updateHistoryFlags();
  }, [persist, repaint, updateHistoryFlags]);

  const redo = useCallback(() => {
    const next = futureRef.current.pop();
    if (!next) return;
    pastRef.current.push(cloneDrawings(stateRef.current.drawings));
    stateRef.current.drawings = next;
    stateRef.current.selectedId = null;
    persist();
    repaint();
    updateHistoryFlags();
  }, [persist, repaint, updateHistoryFlags]);

  const pointAt = useCallback(
    (clientX: number, clientY: number): DrawingPoint | null => {
      const chart = chartRef.current;
      const series = seriesRef.current;
      const container = containerRef.current;
      if (!chart || !series || !container) return null;

      const rect = container.getBoundingClientRect();
      const x = clientX - rect.left;
      const y = clientY - rect.top;
      const logical = chart.timeScale().coordinateToLogical(x);
      const price = series.coordinateToPrice(y);
      if (logical === null || price === null) return null;

      let point: DrawingPoint = { logical: logical as number, price };
      if (magnetRef.current && candlesRef.current?.length) {
        const candles = candlesRef.current;
        const index = Math.max(0, Math.min(candles.length - 1, Math.round(point.logical)));
        const bar = candles[index];
        const ohlc = [bar.open, bar.high, bar.low, bar.close];
        let snapped = ohlc[0];
        for (const v of ohlc) if (Math.abs(v - price) < Math.abs(snapped - price)) snapped = v;
        point = { logical: index, price: snapped };
      }
      return point;
    },
    [chartRef, seriesRef, containerRef, candlesRef],
  );

  const hitTest = useCallback(
    (clientX: number, clientY: number): string | null => {
      const container = containerRef.current;
      const primitive = primitiveRef.current;
      if (!container || !primitive) return null;
      const rect = container.getBoundingClientRect();
      const x = clientX - rect.left;
      const y = clientY - rect.top;

      // Topmost (last drawn) wins.
      for (let i = stateRef.current.drawings.length - 1; i >= 0; i--) {
        const d = stateRef.current.drawings[i];
        const coords = d.points.map((p) => primitive.coord(p));
        if (coords.some((c) => c === null)) continue;
        const cs = coords as { x: number; y: number }[];
        let hit = false;

        if (d.tool === 'horizontal') hit = Math.abs(y - cs[0].y) <= HIT_THRESHOLD;
        else if (d.tool === 'vertical') hit = Math.abs(x - cs[0].x) <= HIT_THRESHOLD;
        else if (d.tool === 'text') hit = Math.hypot(x - cs[0].x, y - cs[0].y) <= 16;
        else if (d.tool === 'rectangle' || d.tool === 'ellipse' || d.tool === 'measure') {
          const minX = Math.min(cs[0].x, cs[1].x);
          const maxX = Math.max(cs[0].x, cs[1].x);
          const minY = Math.min(cs[0].y, cs[1].y);
          const maxY = Math.max(cs[0].y, cs[1].y);
          hit = x >= minX - HIT_THRESHOLD && x <= maxX + HIT_THRESHOLD && y >= minY - HIT_THRESHOLD && y <= maxY + HIT_THRESHOLD;
        } else {
          for (let j = 0; j < cs.length - 1; j++) {
            if (distanceToSegment(x, y, cs[j].x, cs[j].y, cs[j + 1].x, cs[j + 1].y) <= HIT_THRESHOLD) {
              hit = true;
              break;
            }
          }
        }
        if (hit) return d.id;
      }
      return null;
    },
    [containerRef],
  );

  const addDrawing = useCallback(
    (drawing: Omit<Drawing, 'id' | 'color'> & { color?: string }) => {
      snapshot();
      stateRef.current.drawings.push({
        ...drawing,
        id: crypto.randomUUID(),
        color: drawing.color ?? DEFAULT_COLOR,
      });
      persist();
      repaint();
    },
    [snapshot, persist, repaint],
  );

  const setActiveTool = useCallback(
    (tool: DrawingToolId) => {
      activeToolRef.current = tool;
      setActiveToolState(tool);
      stateRef.current.selectedId = null;
      stateRef.current.draft = null;
      const drawingMode = tool !== 'cursor';
      chartRef.current?.applyOptions({ handleScroll: !drawingMode, handleScale: !drawingMode });
      repaint();
    },
    [chartRef, repaint],
  );

  const toggleMagnet = useCallback(() => {
    magnetRef.current = !magnetRef.current;
    setMagnet(magnetRef.current);
  }, []);

  const toggleHidden = useCallback(() => {
    stateRef.current.hidden = !stateRef.current.hidden;
    setHidden(stateRef.current.hidden);
    repaint();
  }, [repaint]);

  const clearAll = useCallback(() => {
    if (stateRef.current.drawings.length === 0) return;
    snapshot();
    stateRef.current.drawings = [];
    stateRef.current.selectedId = null;
    persist();
    repaint();
  }, [snapshot, persist, repaint]);

  // Attach the primitive and input listeners once the chart is ready.
  useEffect(() => {
    if (!ready) return;
    const series = seriesRef.current;
    const container = containerRef.current;
    if (!series || !container) return;

    const primitive = new DrawingPrimitive(() => stateRef.current);
    series.attachPrimitive(primitive);
    primitiveRef.current = primitive;

    function onMouseDown(e: MouseEvent) {
      if (e.button !== 0) return;
      const tool = activeToolRef.current;
      const point = pointAt(e.clientX, e.clientY);
      if (!point) return;

      if (tool === 'cursor') {
        const id = hitTest(e.clientX, e.clientY);
        stateRef.current.selectedId = id;
        if (id) {
          const target = stateRef.current.drawings.find((d) => d.id === id);
          if (target) {
            moveRef.current = {
              id,
              startLogical: point.logical,
              startPrice: point.price,
              origin: target.points.map((p) => ({ ...p })),
              moved: false,
            };
            // Stop the chart from panning while dragging a drawing.
            chartRef.current?.applyOptions({ handleScroll: false, handleScale: false });
          }
        }
        repaint();
        return;
      }
      if (tool === 'text') {
        const text = window.prompt('Texto de la anotación:');
        if (text) addDrawing({ tool: 'text', points: [point], text });
        setActiveTool('cursor');
        return;
      }
      if (SINGLE_POINT_TOOLS.has(tool)) {
        addDrawing({ tool, points: [point] });
        setActiveTool('cursor');
        return;
      }
      stateRef.current.draft = {
        id: 'draft',
        tool,
        points: tool === 'brush' ? [point] : [point, point],
        color: DEFAULT_COLOR,
      };
      repaint();
    }

    function onMouseMove(e: MouseEvent) {
      const move = moveRef.current;
      if (move) {
        const point = pointAt(e.clientX, e.clientY);
        if (!point) return;
        const dLogical = point.logical - move.startLogical;
        const dPrice = point.price - move.startPrice;
        const target = stateRef.current.drawings.find((d) => d.id === move.id);
        if (target) {
          if (!move.moved) {
            // Record the pre-move state once, on the first actual drag.
            snapshot();
            move.moved = true;
          }
          target.points = move.origin.map((p) => ({ logical: p.logical + dLogical, price: p.price + dPrice }));
          repaint();
        }
        return;
      }
      const draft = stateRef.current.draft;
      if (!draft) return;
      const point = pointAt(e.clientX, e.clientY);
      if (!point) return;
      if (draft.tool === 'brush') draft.points.push(point);
      else draft.points[1] = point;
      repaint();
    }

    function onMouseUp() {
      if (moveRef.current) {
        const moved = moveRef.current.moved;
        moveRef.current = null;
        chartRef.current?.applyOptions({ handleScroll: true, handleScale: true });
        if (moved) persist();
        return;
      }
      const draft = stateRef.current.draft;
      if (!draft) return;
      stateRef.current.draft = null;
      if (draft.tool === 'brush') {
        if (draft.points.length > 1) addDrawing({ tool: 'brush', points: draft.points });
      } else {
        const [a, b] = draft.points;
        if (a.logical !== b.logical || a.price !== b.price) addDrawing({ tool: draft.tool, points: [a, b] });
      }
      setActiveTool('cursor');
    }

    function onKeyDown(e: KeyboardEvent) {
      if ((e.metaKey || e.ctrlKey) && (e.key === 'z' || e.key === 'Z')) {
        e.preventDefault();
        if (e.shiftKey) redo();
        else undo();
        return;
      }
      if (e.key === 'Escape') {
        stateRef.current.draft = null;
        setActiveTool('cursor');
        return;
      }
      if ((e.key === 'Delete' || e.key === 'Backspace') && stateRef.current.selectedId) {
        snapshot();
        stateRef.current.drawings = stateRef.current.drawings.filter(
          (d) => d.id !== stateRef.current.selectedId,
        );
        stateRef.current.selectedId = null;
        persist();
        repaint();
      }
    }

    container.addEventListener('mousedown', onMouseDown);
    window.addEventListener('mousemove', onMouseMove);
    window.addEventListener('mouseup', onMouseUp);
    window.addEventListener('keydown', onKeyDown);

    return () => {
      series.detachPrimitive(primitive);
      primitiveRef.current = null;
      container.removeEventListener('mousedown', onMouseDown);
      window.removeEventListener('mousemove', onMouseMove);
      window.removeEventListener('mouseup', onMouseUp);
      window.removeEventListener('keydown', onKeyDown);
    };
  }, [ready, seriesRef, containerRef, chartRef, pointAt, hitTest, addDrawing, setActiveTool, persist, repaint, snapshot, undo, redo]);

  // Load persisted drawings whenever the symbol changes.
  useEffect(() => {
    if (!ready) return;
    symbolRef.current = symbol;
    stateRef.current.drawings = DrawingRepository.load(symbol);
    stateRef.current.selectedId = null;
    stateRef.current.draft = null;
    pastRef.current = [];
    futureRef.current = [];
    updateHistoryFlags();
    setHasDrawings(stateRef.current.drawings.length > 0);
    repaint();
  }, [ready, symbol, repaint, updateHistoryFlags]);

  return {
    activeTool,
    setActiveTool,
    magnet,
    toggleMagnet,
    hidden,
    toggleHidden,
    hasDrawings,
    clearAll,
    undo,
    redo,
    canUndo,
    canRedo,
  };
}
