import type { Drawing } from '@models/Drawing';

const PREFIX = 'tf_drawings_';

/**
 * Persists chart drawings per symbol in localStorage so a user's annotations
 * survive reloads. There is no backend store for this (frontend-only feature).
 */
export const DrawingRepository = {
  load(symbol: string): Drawing[] {
    if (!symbol) return [];
    const raw = localStorage.getItem(PREFIX + symbol);
    if (!raw) return [];
    try {
      const parsed = JSON.parse(raw);
      return Array.isArray(parsed) ? (parsed as Drawing[]) : [];
    } catch {
      return [];
    }
  },

  save(symbol: string, drawings: Drawing[]): void {
    if (!symbol) return;
    localStorage.setItem(PREFIX + symbol, JSON.stringify(drawings));
  },
};
