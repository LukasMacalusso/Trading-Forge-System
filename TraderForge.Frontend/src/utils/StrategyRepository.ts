import type { Edge, Node } from '@xyflow/react';
import type { BotNodeData } from '@models/BotFlow';

export type StrategyNode = Node<BotNodeData>;
export type WorkspaceStatus = 'draft' | 'active';

/** Serializable snapshot of a strategy as drawn in the builder canvas. */
export interface StrategySnapshot {
  name: string;
  status: WorkspaceStatus;
  nodes: StrategyNode[];
  edges: Edge[];
  savedAt: string;
}

const STRATEGY_KEY = 'tf_strategy_draft';

/**
 * Abstracts localStorage access for the Strategy Builder. Backend persistence
 * (4.1 Workflow Engine) is not available yet, so the workspace is saved locally
 * as mock data — see TFS-23 notes.
 */
export const StrategyRepository = {
  load(): StrategySnapshot | null {
    const raw = localStorage.getItem(STRATEGY_KEY);
    if (!raw) return null;
    try {
      return JSON.parse(raw) as StrategySnapshot;
    } catch {
      return null;
    }
  },

  save(snapshot: StrategySnapshot): void {
    localStorage.setItem(STRATEGY_KEY, JSON.stringify(snapshot));
  },

  clear(): void {
    localStorage.removeItem(STRATEGY_KEY);
  },
};
