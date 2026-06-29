export type BotType = 'Analysis' | 'Notification' | 'Action';
export type BotFlowStatus = 'Active' | 'Paused' | 'Stopped' | 'WaitingAuthorization';
export type ActionType = 'Buy' | 'Sell';
export type ConditionOperator = '>' | '<' | '>=' | '<=' | '==';

export interface AnalysisBotConfig {
  symbol: string;
  indicatorType: 'Price' | 'RSI' | 'MA';
  operator: ConditionOperator;
  targetValue: number;
}

export interface NotificationBotConfig {
  message: string;
}

export interface ActionBotConfig {
  action: ActionType;
  symbol: string;
  quantity: number;
}

export type BotConfig = AnalysisBotConfig | NotificationBotConfig | ActionBotConfig;

/** Canvas node kind used by the Strategy Builder (React Flow node `type`). */
export type BotNodeKind = 'analysisBot' | 'notificationBot' | 'actionBot';

/**
 * Discriminated data carried by each canvas node. The `kind` discriminant lets
 * the UI narrow `config` to the correct shape without casts.
 */
export type BotNodeData =
  | { kind: 'analysisBot'; label: string; config: AnalysisBotConfig }
  | { kind: 'notificationBot'; label: string; config: NotificationBotConfig }
  | { kind: 'actionBot'; label: string; config: ActionBotConfig };

export interface BotNode {
  id: string;
  type: BotType;
  label: string;
  config: BotConfig;
}

export interface BotFlow {
  id: string;
  name: string;
  status: BotFlowStatus;
  nodes: BotNode[];
  createdAt: string;
  lastTriggeredAt?: string;
}

export interface PendingOperation {
  id: string;
  flowId: string;
  flowName: string;
  symbol: string;
  action: ActionType;
  quantity: number;
  currentPrice: number;
  conditionMetAt: string;
  expiresAt: string;
}
