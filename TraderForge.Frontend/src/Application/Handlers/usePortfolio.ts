import { useEffect } from 'react';
import { usePortfolioStore } from '../Store/portfolioStore';
import { PortfolioService } from '../../Infrastructure/Services/PortfolioService';
import { TradingService } from '../../Infrastructure/Services/TradingService';
import { useNotificationStore } from '../Store/notificationStore';

const portfolioService = new PortfolioService();
const tradingService = new TradingService();

export function usePortfolio(traderId = 'mock-trader-id') {
  const { portfolio, orderHistory, simulationHistory, isLoading,
    setPortfolio, setOrderHistory, setSimulationHistory, setLoading } = usePortfolioStore();
  const { addNotification } = useNotificationStore();

  useEffect(() => {
    setLoading(true);
    Promise.all([
      portfolioService.getPortfolio(traderId),
      portfolioService.getSimulationHistory(traderId),
      tradingService.getOrderHistory(traderId),
    ]).then(([portfolioResult, historyResult, ordersResult]) => {
      if (portfolioResult.isSuccess) setPortfolio(portfolioResult.value!);
      if (historyResult.isSuccess) setSimulationHistory(historyResult.value!);
      if (ordersResult.isSuccess) setOrderHistory(ordersResult.value!);
      setLoading(false);
    });
  }, [traderId]);

  async function resetSimulation() {
    const result = await portfolioService.resetSimulation(traderId);
    if (result.isSuccess) {
      addNotification('success', 'Simulation reset. Balance restored to plan default.');
      const refreshed = await portfolioService.getPortfolio(traderId);
      if (refreshed.isSuccess) setPortfolio(refreshed.value!);
    } else {
      addNotification('error', result.errorMessage ?? 'Reset failed.');
    }
  }

  return { portfolio, orderHistory, simulationHistory, isLoading, resetSimulation };
}
