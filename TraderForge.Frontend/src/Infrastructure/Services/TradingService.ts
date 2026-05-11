import type { Order } from '../../Domain/Entities/Order';
import { Result } from '../../Application/Common/Result';
import { httpClient } from '../Http/httpClient';

interface BackendOrder {
  id: string;
  symbol: string;
  side: string;
  type: string;
  quantity: number;
  price: number;
  commission: number;
  total: number;
  status: string;
  createdAt: string;
  filledAt: string | null;
}

export class TradingService {
  async getOrderHistory(): Promise<Result<Order[]>> {
    try {
      const { data } = await httpClient.get<BackendOrder[]>('/api/portfolio/orders');

      const orders: Order[] = data.map((o) => ({
        id: o.id,
        symbol: o.symbol,
        side: o.side as Order['side'],
        type: o.type as Order['type'],
        quantity: o.quantity,
        price: o.price,
        commission: o.commission,
        total: o.total,
        status: o.status as Order['status'],
        createdAt: o.createdAt,
        filledAt: o.filledAt ?? undefined,
      }));

      return Result.ok(orders);
    } catch (error) {
      const e = error as { response?: { data?: { error?: string } }; code?: string };
      if (e?.code === 'ERR_NETWORK' || !e?.response) return Result.fail('Cannot reach the server.');
      if (e?.response?.data?.error) return Result.fail(e.response.data.error);
      return Result.fail('Failed to load order history.');
    }
  }
}
