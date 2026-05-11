import { useState } from 'react';
import { PortfolioService } from '../../Infrastructure/Services/PortfolioService';
import { SubscriptionService } from '../../Infrastructure/Services/SubscriptionService';
import { usePortfolioStore } from '../Store/portfolioStore';
import { useNotificationStore } from '../Store/notificationStore';
import type { PlaceOrderCommand } from '../DTOs/Commands/PlaceOrderCommand';
import { COMMISSION_RATE } from '../Common/constants';

const portfolioService = new PortfolioService();
const subscriptionService = new SubscriptionService();

export function usePlaceOrder() {
  const [isSubmitting, setIsSubmitting] = useState(false);
  const { portfolio, setPortfolio } = usePortfolioStore();
  const { addNotification } = useNotificationStore();

  function validate(command: PlaceOrderCommand, currentPrice: number): string | null {
    if (!portfolio) return 'Portfolio not loaded.';
    if (command.quantity <= 0) return 'La cantidad debe ser mayor a 0.';
    if (command.side === 'Buy') {
      const estimatedTotal = currentPrice * command.quantity * (1 + COMMISSION_RATE);
      if (estimatedTotal > portfolio.virtualBalance) {
        return `Balance insuficiente. Necesitas $${estimatedTotal.toFixed(2)}, tienes $${portfolio.virtualBalance.toFixed(2)}`;
      }
    }
    if (command.side === 'Sell') {
      const position = portfolio.positions.find((p) => p.symbol === command.symbol);
      if (!position) return `No tienes posición en ${command.symbol}.`;
    }
    return null;
  }

  async function refreshPortfolio() {
    const planResult = await subscriptionService.getMyPlan();
    const initialBalance = planResult.isSuccess ? planResult.value!.initialVirtualBalance : 10_000;
    const refreshed = await portfolioService.getPortfolio(initialBalance);
    if (refreshed.isSuccess) setPortfolio(refreshed.value!);
  }

  async function placeOrder(command: PlaceOrderCommand, currentPrice: number): Promise<boolean> {
    const validationError = validate(command, currentPrice);
    if (validationError) {
      addNotification('error', validationError);
      return false;
    }

    setIsSubmitting(true);

    let result: { isSuccess: boolean; errorMessage?: string };

    if (command.side === 'Buy') {
      const entryPrice = command.limitPrice ?? currentPrice;
      result = await portfolioService.addAsset(command.symbol, command.quantity, entryPrice);
      if (result.isSuccess) {
        addNotification('success', `Compra de ${command.quantity} ${command.symbol} a $${entryPrice.toFixed(2)} ejecutada`);
      }
    } else {
      const position = portfolio!.positions.find((p) => p.symbol === command.symbol)!;
      result = await portfolioService.removeAsset(position.id);
      if (result.isSuccess) {
        addNotification('success', `Posición en ${command.symbol} cerrada`);
      }
    }

    if (!result.isSuccess) {
      addNotification('error', result.errorMessage ?? 'La orden falló.');
      setIsSubmitting(false);
      return false;
    }

    await refreshPortfolio();
    setIsSubmitting(false);
    return true;
  }

  return { placeOrder, isSubmitting };
}
