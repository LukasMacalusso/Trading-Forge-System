import type { CandlestickBar } from '@models/Asset';

export interface IndicatorPoint {
  time: number;
  value: number;
}

/**
 * Simple Moving Average of closing prices. The first `period - 1` bars have no
 * value and are omitted so the line only starts once enough data exists.
 */
export function sma(candles: CandlestickBar[], period: number): IndicatorPoint[] {
  if (period <= 0 || candles.length < period) return [];

  const out: IndicatorPoint[] = [];
  let sum = 0;

  for (let i = 0; i < candles.length; i++) {
    sum += candles[i].close;
    if (i >= period) sum -= candles[i - period].close;
    if (i >= period - 1) out.push({ time: candles[i].time, value: sum / period });
  }

  return out;
}

/** Exponential Moving Average, seeded with the SMA of the first `period` bars. */
export function ema(candles: CandlestickBar[], period: number): IndicatorPoint[] {
  if (period <= 0 || candles.length < period) return [];

  const k = 2 / (period + 1);
  const out: IndicatorPoint[] = [];

  let seed = 0;
  for (let i = 0; i < period; i++) seed += candles[i].close;
  let prev = seed / period;
  out.push({ time: candles[period - 1].time, value: prev });

  for (let i = period; i < candles.length; i++) {
    prev = candles[i].close * k + prev * (1 - k);
    out.push({ time: candles[i].time, value: prev });
  }

  return out;
}

/**
 * Relative Strength Index using Wilder's smoothing. Returns values in [0, 100]
 * starting at bar index `period`.
 */
export function rsi(candles: CandlestickBar[], period = 14): IndicatorPoint[] {
  if (candles.length <= period) return [];

  const toRsi = (avgGain: number, avgLoss: number): number =>
    avgLoss === 0 ? 100 : 100 - 100 / (1 + avgGain / avgLoss);

  let gain = 0;
  let loss = 0;
  for (let i = 1; i <= period; i++) {
    const change = candles[i].close - candles[i - 1].close;
    if (change >= 0) gain += change;
    else loss -= change;
  }

  let avgGain = gain / period;
  let avgLoss = loss / period;

  const out: IndicatorPoint[] = [{ time: candles[period].time, value: toRsi(avgGain, avgLoss) }];

  for (let i = period + 1; i < candles.length; i++) {
    const change = candles[i].close - candles[i - 1].close;
    const g = change > 0 ? change : 0;
    const l = change < 0 ? -change : 0;
    avgGain = (avgGain * (period - 1) + g) / period;
    avgLoss = (avgLoss * (period - 1) + l) / period;
    out.push({ time: candles[i].time, value: toRsi(avgGain, avgLoss) });
  }

  return out;
}
