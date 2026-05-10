export interface Position {
  symbol: string;
  assetName: string;
  quantity: number;
  averageBuyPrice: number;
  currentPrice: number;
  unrealizedPnL: number;
  unrealizedPnLPercent: number;
  totalValue: number;
}


export interface Portfolio {
  traderId: string;
  virtualBalance: number;
  totalPortfolioValue: number;
  totalPnL: number;
  totalPnLPercent: number;
  positions: Position[];
}

export interface SimulationSnapshot {
  id: string;
  createdAt: string;
  finalBalance: number;
  finalPortfolioValue: number;
  totalPnL: number;
  totalPnLPercent: number;
  positionCount: number;
}
