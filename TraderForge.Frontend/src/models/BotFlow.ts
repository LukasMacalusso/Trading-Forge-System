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

export interface ActionBotConfig {
  action: ActionType;
  symbol: string;
  quantity: number;
}

export interface BotNode {
  id: string;
  type: BotType;
  label: string;
  config: AnalysisBotConfig | ActionBotConfig | Record<string, unknown>;
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
