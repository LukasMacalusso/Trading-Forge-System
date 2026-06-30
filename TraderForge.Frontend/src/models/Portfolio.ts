export interface Position {
  id: string;
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

/** A single balance movement in the portfolio ledger. */
export interface Transaction {
  id: string;
  type: string;
  symbol?: string | null;
  quantity?: number | null;
  price?: number | null;
  commission: number;
  total: number;
  balanceBefore: number;
  balanceAfter: number;
  createdAt: string;
}
