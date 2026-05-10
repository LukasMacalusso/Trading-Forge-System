import type { Order } from '../../Domain/Entities/Order';
import { Result } from '../../Application/Common/Result';
import type { PlaceOrderCommand } from '../../Application/DTOs/Commands/PlaceOrderCommand';
import { COMMISSION_RATE } from '../../Application/Common/constants';

const MOCK_PRICES: Record<string, number> = {
  AAPL: 189.84, TSLA: 248.50, MSFT: 415.20, GOOGL: 175.60,
  AMZN: 192.30, NVDA: 875.40, META: 521.00, BTC: 63200,
};

const MOCK_HISTORY: Order[] = [
  { id: '1', symbol: 'AAPL', side: 'Buy', type: 'Market', quantity: 10, price: 182.50, commission: 1.83, total: 1826.83, status: 'Filled', createdAt: '2026-04-28T10:30:00Z', filledAt: '2026-04-28T10:30:01Z' },
  { id: '2', symbol: 'TSLA', side: 'Buy', type: 'Limit', quantity: 5, price: 245.00, commission: 1.23, total: 1226.23, status: 'Filled', createdAt: '2026-04-29T14:15:00Z', filledAt: '2026-04-29T15:00:00Z' },
  { id: '3', symbol: 'AAPL', side: 'Sell', type: 'Market', quantity: 5, price: 189.84, commission: 0.95, total: 948.25, status: 'Filled', createdAt: '2026-05-01T09:32:00Z', filledAt: '2026-05-01T09:32:01Z' },
];

/** Mock implementation — replace with real API calls when backend trading endpoints are ready. */
export class TradingService {
  private delay = (ms: number) => new Promise((r) => setTimeout(r, ms));

  async placeOrder(command: PlaceOrderCommand): Promise<Result<Order>> {
    await this.delay(500);
    const price = command.limitPrice ?? MOCK_PRICES[command.symbol] ?? 100;
    const commission = +(price * command.quantity * COMMISSION_RATE).toFixed(2);
    const total = command.side === 'Buy'
      ? +(price * command.quantity + commission).toFixed(2)
      : +(price * command.quantity - commission).toFixed(2);

    const order: Order = {
      id: crypto.randomUUID(),
      symbol: command.symbol,
      side: command.side,
      type: command.type,
      quantity: command.quantity,
      price,
      commission,
      total,
      status: 'Filled',
      createdAt: new Date().toISOString(),
      filledAt: new Date().toISOString(),
    };
    MOCK_HISTORY.unshift(order);
    return Result.ok(order);
  }

  async getOrderHistory(_traderId: string): Promise<Result<Order[]>> {
    await this.delay(200);
    return Result.ok(MOCK_HISTORY);
  }

  async cancelOrder(orderId: string): Promise<Result<void>> {
    await this.delay(150);
    const idx = MOCK_HISTORY.findIndex((o) => o.id === orderId);
    if (idx !== -1) MOCK_HISTORY[idx].status = 'Cancelled';
    return Result.ok(undefined);
  }
}
