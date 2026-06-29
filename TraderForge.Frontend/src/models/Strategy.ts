import type { Edge, Node } from '@xyflow/react';
import type { BotNodeData } from './BotFlow';

export type StrategyStatus = 'draft' | 'active' | 'paused';

/** A canvas node (a bot) inside a strategy. */
export type StrategyNode = Node<BotNodeData>;

/**
 * A strategy is the environment where bots (nodes) are connected. It is the unit
 * the user creates and manages from the "My Strategies" page (TFS-49).
 */
export interface Strategy {
  id: string;
  name: string;
  status: StrategyStatus;
  nodes: StrategyNode[];
  edges: Edge[];
  createdAt: string;
  updatedAt: string;
}
