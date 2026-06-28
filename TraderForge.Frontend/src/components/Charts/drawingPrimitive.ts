import type {
  ISeriesPrimitive,
  IPrimitivePaneView,
  IPrimitivePaneRenderer,
  SeriesAttachedParameter,
  IChartApi,
  ISeriesApi,
  Time,
} from 'lightweight-charts';
import type { CanvasRenderingTarget2D } from 'fancy-canvas';
import type { Drawing, DrawingPoint } from '@models/Drawing';

const FIB_LEVELS = [0, 0.236, 0.382, 0.5, 0.618, 0.786, 1];

export interface DrawingRenderState {
  drawings: Drawing[];
  draft: Drawing | null;
  selectedId: string | null;
  hidden: boolean;
}

interface Coord {
  x: number;
  y: number;
}

/**
 * A single lightweight-charts primitive that renders every drawing (plus the
 * one currently being drawn) onto the price pane. State is read lazily through
 * a getter so the React layer can mutate it and call `requestUpdate()`.
 */
export class DrawingPrimitive implements ISeriesPrimitive<Time> {
  private _chart: IChartApi | null = null;
  private _series: ISeriesApi<'Candlestick'> | null = null;
  private _requestUpdate?: () => void;
  private readonly _paneViews: DrawingPaneView[];

  constructor(private readonly _getState: () => DrawingRenderState) {
    this._paneViews = [new DrawingPaneView(this)];
  }

  attached(param: SeriesAttachedParameter<Time>): void {
    this._chart = param.chart;
    this._series = param.series as ISeriesApi<'Candlestick'>;
    this._requestUpdate = param.requestUpdate;
  }

  detached(): void {
    this._chart = null;
    this._series = null;
    this._requestUpdate = undefined;
  }

  updateAllViews(): void {}

  paneViews(): readonly IPrimitivePaneView[] {
    return this._paneViews;
  }

  requestUpdate(): void {
    this._requestUpdate?.();
  }

  get state(): DrawingRenderState {
    return this._getState();
  }

  coord(point: DrawingPoint): Coord | null {
    if (!this._chart || !this._series) return null;
    const x = this._chart.timeScale().timeToCoordinate(point.time as Time);
    const y = this._series.priceToCoordinate(point.price);
    if (x === null || y === null) return null;
    return { x, y };
  }

  priceToY(price: number): number | null {
    return this._series?.priceToCoordinate(price) ?? null;
  }
}

class DrawingPaneView implements IPrimitivePaneView {
  constructor(private readonly _source: DrawingPrimitive) {}

  renderer(): IPrimitivePaneRenderer {
    return new DrawingRenderer(this._source);
  }
}

class DrawingRenderer implements IPrimitivePaneRenderer {
  constructor(private readonly _source: DrawingPrimitive) {}

  draw(target: CanvasRenderingTarget2D): void {
    const { drawings, draft, selectedId, hidden } = this._source.state;
    if (hidden) return;

    target.useMediaCoordinateSpace((scope) => {
      const ctx = scope.context;
      const width = scope.mediaSize.width;
      const height = scope.mediaSize.height;

      for (const drawing of drawings) {
        this._drawOne(ctx, width, height, drawing, drawing.id === selectedId);
      }
      if (draft) this._drawOne(ctx, width, height, draft, false);
    });
  }

  private _drawOne(
    ctx: CanvasRenderingContext2D,
    width: number,
    height: number,
    drawing: Drawing,
    selected: boolean,
  ): void {
    const pts = drawing.points.map((p) => this._source.coord(p));
    if (pts.some((p) => p === null)) return;
    const coords = pts as Coord[];

    ctx.save();
    ctx.lineWidth = selected ? 2.5 : 1.75;
    ctx.strokeStyle = drawing.color;
    ctx.fillStyle = drawing.color;
    ctx.lineJoin = 'round';
    ctx.lineCap = 'round';

    switch (drawing.tool) {
      case 'trendline':
        this._line(ctx, coords[0], coords[1]);
        break;
      case 'ray':
        this._ray(ctx, coords[0], coords[1], width);
        break;
      case 'arrow':
        this._line(ctx, coords[0], coords[1]);
        this._arrowHead(ctx, coords[0], coords[1]);
        break;
      case 'horizontal':
        this._segment(ctx, 0, coords[0].y, width, coords[0].y);
        break;
      case 'vertical':
        this._segment(ctx, coords[0].x, 0, coords[0].x, height);
        break;
      case 'rectangle':
        this._rect(ctx, coords[0], coords[1], true);
        break;
      case 'measure':
        this._measure(ctx, coords[0], coords[1], drawing);
        break;
      case 'ellipse':
        this._ellipse(ctx, coords[0], coords[1]);
        break;
      case 'brush':
        this._polyline(ctx, coords);
        break;
      case 'text':
        this._text(ctx, coords[0], drawing.text ?? '');
        break;
      case 'fib':
        this._fib(ctx, coords[0], coords[1], drawing, width);
        break;
    }

    if (selected) this._handles(ctx, coords);
    ctx.restore();
  }

  private _segment(ctx: CanvasRenderingContext2D, x1: number, y1: number, x2: number, y2: number): void {
    ctx.beginPath();
    ctx.moveTo(x1, y1);
    ctx.lineTo(x2, y2);
    ctx.stroke();
  }

  private _line(ctx: CanvasRenderingContext2D, a: Coord, b: Coord): void {
    this._segment(ctx, a.x, a.y, b.x, b.y);
  }

  private _ray(ctx: CanvasRenderingContext2D, a: Coord, b: Coord, width: number): void {
    const dx = b.x - a.x;
    const dy = b.y - a.y;
    const farX = dx === 0 ? a.x : width + 100;
    const farY = dx === 0 ? (dy >= 0 ? 100000 : -100000) : a.y + (dy / dx) * (farX - a.x);
    this._segment(ctx, a.x, a.y, farX, farY);
  }

  private _arrowHead(ctx: CanvasRenderingContext2D, a: Coord, b: Coord): void {
    const angle = Math.atan2(b.y - a.y, b.x - a.x);
    const size = 11;
    ctx.beginPath();
    ctx.moveTo(b.x, b.y);
    ctx.lineTo(b.x - size * Math.cos(angle - Math.PI / 6), b.y - size * Math.sin(angle - Math.PI / 6));
    ctx.lineTo(b.x - size * Math.cos(angle + Math.PI / 6), b.y - size * Math.sin(angle + Math.PI / 6));
    ctx.closePath();
    ctx.fill();
  }

  private _rect(ctx: CanvasRenderingContext2D, a: Coord, b: Coord, fill: boolean): void {
    const x = Math.min(a.x, b.x);
    const y = Math.min(a.y, b.y);
    const w = Math.abs(b.x - a.x);
    const h = Math.abs(b.y - a.y);
    if (fill) {
      ctx.globalAlpha = 0.08;
      ctx.fillRect(x, y, w, h);
      ctx.globalAlpha = 1;
    }
    ctx.strokeRect(x, y, w, h);
  }

  private _ellipse(ctx: CanvasRenderingContext2D, a: Coord, b: Coord): void {
    const cx = (a.x + b.x) / 2;
    const cy = (a.y + b.y) / 2;
    const rx = Math.abs(b.x - a.x) / 2;
    const ry = Math.abs(b.y - a.y) / 2;
    ctx.beginPath();
    ctx.ellipse(cx, cy, rx, ry, 0, 0, Math.PI * 2);
    ctx.globalAlpha = 0.08;
    ctx.fill();
    ctx.globalAlpha = 1;
    ctx.stroke();
  }

  private _polyline(ctx: CanvasRenderingContext2D, coords: Coord[]): void {
    if (coords.length < 2) return;
    ctx.beginPath();
    ctx.moveTo(coords[0].x, coords[0].y);
    for (let i = 1; i < coords.length; i++) ctx.lineTo(coords[i].x, coords[i].y);
    ctx.stroke();
  }

  private _text(ctx: CanvasRenderingContext2D, a: Coord, text: string): void {
    if (!text) return;
    ctx.font = '13px ui-sans-serif, system-ui, sans-serif';
    ctx.textBaseline = 'middle';
    ctx.fillText(text, a.x + 4, a.y);
  }

  private _measure(ctx: CanvasRenderingContext2D, a: Coord, b: Coord, drawing: Drawing): void {
    this._rect(ctx, a, b, true);
    const p0 = drawing.points[0];
    const p1 = drawing.points[1];
    const diff = p1.price - p0.price;
    const pct = p0.price !== 0 ? (diff / p0.price) * 100 : 0;
    const label = `${diff >= 0 ? '+' : ''}${diff.toFixed(2)} (${pct >= 0 ? '+' : ''}${pct.toFixed(2)}%)`;
    ctx.font = '12px ui-sans-serif, system-ui, sans-serif';
    ctx.textBaseline = 'middle';
    ctx.textAlign = 'center';
    ctx.fillText(label, (a.x + b.x) / 2, Math.min(a.y, b.y) - 10);
    ctx.textAlign = 'left';
  }

  private _fib(ctx: CanvasRenderingContext2D, a: Coord, b: Coord, drawing: Drawing, width: number): void {
    const p0 = drawing.points[0].price;
    const p1 = drawing.points[1].price;
    const xStart = Math.min(a.x, b.x);
    ctx.font = '11px ui-sans-serif, system-ui, sans-serif';
    ctx.textBaseline = 'middle';

    for (const level of FIB_LEVELS) {
      const price = p0 + (p1 - p0) * level;
      const y = this._source.priceToY(price);
      if (y === null) continue;
      ctx.globalAlpha = 0.7;
      ctx.setLineDash([4, 3]);
      this._segment(ctx, xStart, y, width, y);
      ctx.setLineDash([]);
      ctx.globalAlpha = 1;
      ctx.fillText(`${(level * 100).toFixed(1)}%  ${price.toFixed(2)}`, xStart + 4, y - 7);
    }
  }

  private _handles(ctx: CanvasRenderingContext2D, coords: Coord[]): void {
    ctx.fillStyle = '#ffffff';
    ctx.strokeStyle = '#f59e0b';
    ctx.lineWidth = 1.5;
    for (const c of coords) {
      ctx.beginPath();
      ctx.arc(c.x, c.y, 4, 0, Math.PI * 2);
      ctx.fill();
      ctx.stroke();
    }
  }
}
