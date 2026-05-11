import { useEffect } from 'react';
import { usePortfolioStore } from '../Store/portfolioStore';
import { PortfolioService } from '../../Infrastructure/Services/PortfolioService';
import { SubscriptionService } from '../../Infrastructure/Services/SubscriptionService';
import { useNotificationStore } from '../Store/notificationStore';

const portfolioService = new PortfolioService();
const subscriptionService = new SubscriptionService();

export function usePortfolio() {
  const { portfolio, orderHistory, simulationHistory, isLoading, setPortfolio, setSimulationHistory, setLoading } = usePortfolioStore();
  const { addNotification } = useNotificationStore();

  useEffect(() => {
    async function load() {
      setLoading(true);
      const [planResult, historyResult] = await Promise.all([
        subscriptionService.getMyPlan(),
        portfolioService.getSimulationHistory(),
      ]);
      const initialBalance = planResult.isSuccess
        ? planResult.value!.initialVirtualBalance
        : 10_000;
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
