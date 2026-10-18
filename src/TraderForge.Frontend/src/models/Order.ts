export type OrderSide = 'Buy' | 'Sell';
export type OrderType = 'Market' | 'Limit';
export type OrderStatus = 'Pending' | 'Filled' | 'Cancelled' | 'Rejected';

export interface Order {
  id: string;
  symbol: string;
  side: OrderSide;
  type: OrderType;
  quantity: number;
  price: number;
  commission: number;
  total: number;
  status: OrderStatus;
  createdAt: string;
  filledAt?: string;
}
