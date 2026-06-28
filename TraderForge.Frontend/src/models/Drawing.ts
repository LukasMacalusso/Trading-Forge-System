/** Drawing tools available in the chart's drawing toolbar. */
export type DrawingToolId =
  | 'cursor'
  | 'trendline'
  | 'horizontal'
  | 'vertical'
  | 'ray'
  | 'rectangle'
  | 'ellipse'
  | 'arrow'
  | 'brush'
  | 'text'
  | 'fib'
  | 'measure';

/** A point anchored to chart data: a time (UNIX seconds) and a price. */
export interface DrawingPoint {
  time: number;
  price: number;
}

export interface Drawing {
  id: string;
  tool: DrawingToolId;
  points: DrawingPoint[];
  color: string;
  text?: string;
}

/** Tools that are committed with a single click (the rest need two points). */
export const SINGLE_POINT_TOOLS: ReadonlySet<DrawingToolId> = new Set([
  'horizontal',
  'vertical',
  'text',
]);
