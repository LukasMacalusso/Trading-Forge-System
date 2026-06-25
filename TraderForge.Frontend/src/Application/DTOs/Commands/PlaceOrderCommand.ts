import type { OrderSide, OrderType } from '../../../Domain/Entities/Order';

export interface PlaceOrderCommand {
  traderId: string;
  symbol: string;
  side: OrderSide;
  type: OrderType;
  quantity: number;
  limitPrice?: number;
}
