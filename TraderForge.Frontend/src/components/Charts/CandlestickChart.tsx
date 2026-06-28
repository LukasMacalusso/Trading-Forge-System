import { useEffect, useRef, useState, memo } from 'react';
import {
  createChart,
  ColorType,
  CrosshairMode,
  CandlestickSeries,
  LineSeries,
  LineStyle,
} from 'lightweight-charts';
import type { UTCTimestamp, IChartApi, ISeriesApi } from 'lightweight-charts';
import type { CandlestickBar } from '@models/Asset';
import { sma, rsi, type IndicatorPoint } from '@utils/indicators';

interface CandlestickChartProps {
  candles: CandlestickBar[];
  symbol: string;
}

const MA_PRESETS = [
  { period: 7, color: '#f59e0b' },
  { period: 25, color: '#3b82f6' },
  { period: 99, color: '#a855f7' },
];
const RSI_PERIOD = 14;
const RSI_COLOR = '#c084fc';

const lineOptions = {
  lineWidth: 2 as const,
  priceLineVisible: false,
  lastValueVisible: false,
  crosshairMarkerVisible: false,
};

function toLineData(points: IndicatorPoint[]) {
  return points.map((p) => ({ time: p.time as UTCTimestamp, value: p.value }));
}

export const CandlestickChart = memo(function CandlestickChart({ candles, symbol }: CandlestickChartProps) {
  const containerRef = useRef<HTMLDivElement>(null);
  const chartRef = useRef<IChartApi | null>(null);
  const candleRef = useRef<ISeriesApi<'Candlestick'> | null>(null);
  const maRefs = useRef<Map<number, ISeriesApi<'Line'>>>(new Map());
  const rsiRef = useRef<ISeriesApi<'Line'> | null>(null);
  const lastSymbol = useRef<string | null>(null);

  const [activeMas, setActiveMas] = useState<number[]>([7, 25]);
  const [showRsi, setShowRsi] = useState(false);

  // Create the chart once and keep it alive so toggling indicators or live
  // price updates never reset the user's pan/zoom.
  useEffect(() => {
    if (!containerRef.current) return;

    const chart = createChart(containerRef.current, {
      autoSize: true,
      layout: {
        background: { type: ColorType.Solid, color: '#0a0a0b' },
        textColor: '#9ca3af',
      },
      grid: {
        vertLines: { color: '#1a1a1f' },
        horzLines: { color: '#1a1a1f' },
      },
      crosshair: { mode: CrosshairMode.Normal },
      rightPriceScale: { borderColor: '#2e303a' },
      timeScale: { borderColor: '#2e303a', timeVisible: true, secondsVisible: false },
    });

    candleRef.current = chart.addSeries(CandlestickSeries, {
      upColor: '#10b981',
      downColor: '#ef4444',
      borderVisible: false,
      wickUpColor: '#10b981',
      wickDownColor: '#ef4444',
    });
    chartRef.current = chart;

    return () => {
      chart.remove();
      chartRef.current = null;
      candleRef.current = null;
      maRefs.current.clear();
      rsiRef.current = null;
      lastSymbol.current = null;
    };
  }, []);

  // Candle data — only refit the view when the symbol changes.
  useEffect(() => {
    const chart = chartRef.current;
    const candle = candleRef.current;
    if (!chart || !candle || candles.length === 0) return;

    candle.setData(candles.map((c) => ({ ...c, time: c.time as UTCTimestamp })));
    if (lastSymbol.current !== symbol) {
      chart.timeScale().fitContent();
      lastSymbol.current = symbol;
    }
  }, [candles, symbol]);

  // Reconcile moving-average overlays with the active selection.
  useEffect(() => {
    const chart = chartRef.current;
    if (!chart) return;
    const map = maRefs.current;

    for (const [period, series] of map) {
      if (!activeMas.includes(period)) {
        chart.removeSeries(series);
        map.delete(period);
      }
    }

    for (const period of activeMas) {
      const preset = MA_PRESETS.find((p) => p.period === period);
      if (!preset) continue;
      let series = map.get(period);
      if (!series) {
        series = chart.addSeries(LineSeries, { ...lineOptions, color: preset.color });
        map.set(period, series);
      }
      series.setData(toLineData(sma(candles, period)));
    }
  }, [candles, activeMas]);

  // Add or remove the RSI sub-pane with its 70/30 reference lines.
  useEffect(() => {
    const chart = chartRef.current;
    if (!chart) return;

    if (showRsi) {
      if (!rsiRef.current) {
        const series = chart.addSeries(LineSeries, { ...lineOptions, color: RSI_COLOR }, 1);
        series.createPriceLine({
          price: 70,
          color: '#ef444466',
          lineStyle: LineStyle.Dashed,
          lineWidth: 1,
          axisLabelVisible: true,
          title: '70',
        });
        series.createPriceLine({
          price: 30,
          color: '#10b98166',
          lineStyle: LineStyle.Dashed,
          lineWidth: 1,
          axisLabelVisible: true,
          title: '30',
        });
        rsiRef.current = series;
        const panes = chart.panes();
        panes[0]?.setStretchFactor(3);
        panes[1]?.setStretchFactor(1);
      }
      rsiRef.current.setData(toLineData(rsi(candles, RSI_PERIOD)));
    } else if (rsiRef.current) {
      chart.removeSeries(rsiRef.current);
      rsiRef.current = null;
    }
  }, [candles, showRsi]);

  function toggleMa(period: number) {
    setActiveMas((prev) =>
      prev.includes(period) ? prev.filter((p) => p !== period) : [...prev, period].sort((a, b) => a - b),
    );
  }

  return (
    <div className="w-full h-full relative">
      {candles.length > 0 && (
        <div className="absolute top-2 left-2 z-10 flex flex-wrap items-center gap-1.5">
          {MA_PRESETS.map((p) => {
            const active = activeMas.includes(p.period);
            return (
              <button
                key={p.period}
                onClick={() => toggleMa(p.period)}
                aria-pressed={active}
                className={`flex items-center gap-1.5 px-2 py-1 rounded-md text-[11px] font-medium border transition-colors ${
                  active
                    ? 'bg-neutral-800 border-neutral-700 text-neutral-200'
                    : 'bg-neutral-900/80 border-neutral-800 text-neutral-500 hover:text-neutral-300'
                }`}
              >
                <span
                  className="w-2 h-2 rounded-full"
                  style={{ background: active ? p.color : '#525252' }}
                />
                MA{p.period}
              </button>
            );
          })}
          <button
            onClick={() => setShowRsi((v) => !v)}
            aria-pressed={showRsi}
            className={`flex items-center gap-1.5 px-2 py-1 rounded-md text-[11px] font-medium border transition-colors ${
              showRsi
                ? 'bg-neutral-800 border-neutral-700 text-neutral-200'
                : 'bg-neutral-900/80 border-neutral-800 text-neutral-500 hover:text-neutral-300'
            }`}
          >
            <span
              className="w-2 h-2 rounded-full"
              style={{ background: showRsi ? RSI_COLOR : '#525252' }}
            />
            RSI
          </button>
        </div>
      )}

      <div ref={containerRef} className="w-full h-full" />

      {candles.length === 0 && (
        <div className="absolute inset-0 flex items-center justify-center text-neutral-600 text-sm pointer-events-none">
          Select an asset to view the chart
        </div>
      )}
    </div>
  );
});
