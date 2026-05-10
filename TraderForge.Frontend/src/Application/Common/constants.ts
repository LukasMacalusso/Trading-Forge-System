export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000';

export const COMMISSION_RATE = 0.001; // 0.1% — BR-1

export const PLAN_LIMITS = {
  FreeTrial: { initialBalance: 10_000, maxBots: 1, maxAssets: 5 },
  Basic:     { initialBalance: 10_000, maxBots: 3, maxAssets: 10 },
  Pro:       { initialBalance: 50_000, maxBots: 10, maxAssets: 25 },
  Enterprise:{ initialBalance: Infinity, maxBots: Infinity, maxAssets: Infinity },
} as const;

export const FREE_TRIAL_DAYS = 7; // BR-10

export const PENDING_OP_TIMEOUT_MINUTES = 15; // BR-18

export const MARKET_DATA_MAX_STALENESS_MS = 60_000; // BR-19 — 1 min refresh
