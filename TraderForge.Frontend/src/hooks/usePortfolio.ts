import { useCallback, useEffect } from 'react';
import { usePortfolioStore } from '@store/portfolioStore';
import { PortfolioService } from '@api/PortfolioService';
import { TradingService } from '@api/TradingService';
import { SubscriptionService } from '@api/SubscriptionService';
import { useNotificationStore } from '@store/notificationStore';

const portfolioService = new PortfolioService();
const subscriptionService = new SubscriptionService();
const tradingService = new TradingService();

export function usePortfolio() {
  const { portfolio, orderHistory, simulationHistory, isLoading, setPortfolio, setOrderHistory, setSimulationHistory, setLoading, setInitialBalance } = usePortfolioStore();
  const { addNotification } = useNotificationStore();

  const load = useCallback(async () => {
    setLoading(true);

    const [planResult, ordersResult] = await Promise.all([
      subscriptionService.getMyPlan(),
      tradingService.getOrderHistory(),
    ]);

    const initialBalance = planResult.isSuccess
      ? planResult.value!.initialVirtualBalance
      : 10_000;
    setInitialBalance(initialBalance);

    // Portfolio and past simulations share the same starting-capital baseline.
    const [portfolioResult, historyResult] = await Promise.all([
      portfolioService.getPortfolio(initialBalance),
      portfolioService.getSimulationHistory(initialBalance),
    ]);

    if (portfolioResult.isSuccess) {
      setPortfolio(portfolioResult.value!);
    } else if (
      !portfolioResult.errorMessage?.includes('Cannot reach') &&
      portfolioResult.errorMessage !== 'UNAUTHORIZED'
    ) {
      addNotification('error', portfolioResult.errorMessage ?? 'Could not load portfolio.');
    }
    if (historyResult.isSuccess) setSimulationHistory(historyResult.value!);
    if (ordersResult.isSuccess) setOrderHistory(ordersResult.value!);
    setLoading(false);
  }, [setLoading, setInitialBalance, setPortfolio, setSimulationHistory, setOrderHistory, addNotification]);

  useEffect(() => {
    load();
  }, [load]);

  /**
   * Resets the simulation on the backend, then reloads the portfolio.
   * Returns whether the reset succeeded so callers can drive their own UI.
   */
  const resetSimulation = useCallback(async (): Promise<boolean> => {
    const result = await portfolioService.resetSimulation();
    if (result.isSuccess) {
      await load();
      addNotification('success', 'Simulación reiniciada. Tu capital virtual fue restaurado.');
      return true;
    }
    addNotification('error', result.errorMessage ?? 'No se pudo reiniciar la simulación.');
    return false;
  }, [load, addNotification]);

  return {
    portfolio,
    orderHistory,
    simulationHistory,
    isLoading,
    resetSimulation,
  };
}
