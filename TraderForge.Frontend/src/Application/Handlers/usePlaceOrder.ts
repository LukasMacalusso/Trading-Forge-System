import { useState } from 'react';
import { TradingService } from '../../Infrastructure/Services/TradingService';
import { PortfolioService } from '../../Infrastructure/Services/PortfolioService';
import { usePortfolioStore } from '../Store/portfolioStore';
import { useNotificationStore } from '../Store/notificationStore';
import type { PlaceOrderCommand } from '../DTOs/Commands/PlaceOrderCommand';
import { COMMISSION_RATE } from '../Common/constants';

const tradingService = new TradingService();
const portfolioService = new PortfolioService();

export function usePlaceOrder() {
  const [isSubmitting, setIsSubmitting] = useState(false);
  const { portfolio, setPortfolio, setOrderHistory, orderHistory } = usePortfolioStore();
  const { addNotification } = useNotificationStore();

  /** Validates BR-1 (commission), BR-6 (zero balance) before submitting. */
  function validate(command: PlaceOrderCommand, currentPrice: number): string | null {
    if (!portfolio) return 'Portfolio not loaded.';
    const estimatedTotal = currentPrice * command.quantity * (1 + COMMISSION_RATE);
    if (command.side === 'Buy' && estimatedTotal > portfolio.virtualBalance) {
      return `Insufficient balance. Required: $${estimatedTotal.toFixed(2)}, Available: $${portfolio.virtualBalance.toFixed(2)}`;
    }
    if (portfolio.virtualBalance <= 0) {
      return 'Account balance is zero. Please reset your simulation. (BR-6)';
    }
    if (command.quantity <= 0) return 'Quantity must be greater than 0.';
    return null;
  }

  async function placeOrder(command: PlaceOrderCommand, currentPrice: number): Promise<boolean> {
    const validationError = validate(command, currentPrice);
    if (validationError) {
      addNotification('error', validationError);
      return false;
    }
    setIsSubmitting(true);
    const result = await tradingService.placeOrder(command);
    setIsSubmitting(false);
    if (!result.isSuccess) {
      addNotification('error', result.errorMessage ?? 'Order failed.');
      return false;
    }
    const order = result.value!;
    addNotification('success', `${order.side} ${order.quantity} ${order.symbol} @ $${order.price.toFixed(2)}`);
    setOrderHistory([order, ...orderHistory]);
    const refreshed = await portfolioService.getPortfolio(command.traderId);
    if (refreshed.isSuccess) setPortfolio(refreshed.value!);
    return true;
  }

  return { placeOrder, isSubmitting };
}
