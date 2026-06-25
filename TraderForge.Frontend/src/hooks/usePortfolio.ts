import { useEffect } from 'react';
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

  useEffect(() => {
    async function load() {
      setLoading(true);

      const [planResult, historyResult, ordersResult] = await Promise.all([
        subscriptionService.getMyPlan(),
        portfolioService.getSimulationHistory(),
        tradingService.getOrderHistory(),
      ]);

      const initialBalance = planResult.isSuccess
        ? planResult.value!.initialVirtualBalance
        : 10_000;
      setInitialBalance(initialBalance);

      const portfolioResult = await portfolioService.getPortfolio(initialBalance);
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
    }
    load();
  }, []);

  return {
    portfolio,
    orderHistory,
    simulationHistory,
    isLoading,
    resetSimulation: () => addNotification('error', 'Reset simulation no está disponible aún.'),
  };
}
