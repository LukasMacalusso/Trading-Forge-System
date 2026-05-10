import { useEffect, useRef, memo } from 'react';
import { createChart, ColorType, CrosshairMode, CandlestickSeries } from 'lightweight-charts';
import type { UTCTimestamp } from 'lightweight-charts';
import type { CandlestickBar } from '../../../Domain/Entities/Asset';

interface CandlestickChartProps {
  candles: CandlestickBar[];
  symbol: string;
}

export const CandlestickChart = memo(function CandlestickChart({ candles, symbol }: CandlestickChartProps) {
  const containerRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (!containerRef.current || candles.length === 0) return;

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

    const series = chart.addSeries(CandlestickSeries, {
      upColor: '#10b981',
      downColor: '#ef4444',
      borderVisible: false,
      wickUpColor: '#10b981',
      wickDownColor: '#ef4444',
    });

    series.setData(candles.map((c) => ({ ...c, time: c.time as UTCTimestamp })));
    chart.timeScale().fitContent();

    return () => chart.remove();
  }, [candles, symbol]);

  if (candles.length === 0) {
    return (
      <div className="w-full h-full flex items-center justify-center text-neutral-600 text-sm">
        Select an asset to view the chart
      </div>
    );
  }

  return <div ref={containerRef} className="w-full h-full" />;
});
