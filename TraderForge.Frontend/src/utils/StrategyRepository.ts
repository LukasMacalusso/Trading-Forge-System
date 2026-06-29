import type { Strategy, StrategyStatus } from '@models/Strategy';

const STRATEGIES_KEY = 'tf_strategies';

function readAll(): Strategy[] {
  const raw = localStorage.getItem(STRATEGIES_KEY);
  if (!raw) return [];
  try {
    const parsed = JSON.parse(raw);
    return Array.isArray(parsed) ? (parsed as Strategy[]) : [];
  } catch {
    return [];
  }
}

function writeAll(list: Strategy[]): void {
  localStorage.setItem(STRATEGIES_KEY, JSON.stringify(list));
}

/**
 * Stores the user's strategies in localStorage. The backend (4.1 Workflow
 * Engine) is barely implemented, so the manager works on mock data — see TFS-49.
 */
export const StrategyRepository = {
  /** Strategies ordered by most recently updated first. */
  list(): Strategy[] {
    return readAll().sort((a, b) => b.updatedAt.localeCompare(a.updatedAt));
  },

  get(id: string): Strategy | null {
    return readAll().find((s) => s.id === id) ?? null;
  },

  create(name: string): Strategy {
    const now = new Date().toISOString();
    const strategy: Strategy = {
      id: crypto.randomUUID(),
      name: name.trim() || 'Nueva estrategia',
      status: 'draft',
      nodes: [],
      edges: [],
      createdAt: now,
      updatedAt: now,
    };
    writeAll([...readAll(), strategy]);
    return strategy;
  },

  /** Inserts or updates a strategy, refreshing its `updatedAt`. */
  upsert(strategy: Strategy): void {
    const list = readAll();
    const next = { ...strategy, updatedAt: new Date().toISOString() };
    const index = list.findIndex((s) => s.id === strategy.id);
    if (index >= 0) list[index] = next;
    else list.push(next);
    writeAll(list);
  },

  setStatus(id: string, status: StrategyStatus): void {
    const list = readAll();
    const index = list.findIndex((s) => s.id === id);
    if (index < 0) return;
    list[index] = { ...list[index], status, updatedAt: new Date().toISOString() };
    writeAll(list);
  },

  duplicate(id: string): Strategy | null {
    const source = readAll().find((s) => s.id === id);
    if (!source) return null;
    const now = new Date().toISOString();
    const copy: Strategy = {
      ...source,
      id: crypto.randomUUID(),
      name: `${source.name} (copia)`,
      status: 'draft',
      createdAt: now,
      updatedAt: now,
    };
    writeAll([...readAll(), copy]);
    return copy;
  },

  remove(id: string): void {
    writeAll(readAll().filter((s) => s.id !== id));
  },
};
