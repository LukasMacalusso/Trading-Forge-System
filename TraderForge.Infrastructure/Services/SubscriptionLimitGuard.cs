using TraderForge.Domain.Entities;
using TraderForge.Domain.Repositories;
using TraderForge.Domain.Services;

namespace TraderForge.Infrastructure.Services;

public class SubscriptionLimitGuard : ISubscriptionLimitGuard
{
    private readonly ITraderRepository _traderRepository;

    public SubscriptionLimitGuard(ITraderRepository traderRepository)
    {
        _traderRepository = traderRepository;
    }

    public async Task<bool> CanAddStrategyAsync(string traderId)
    {
        var trader = await _traderRepository.GetByIdIncludePlanAndStrategyAsync(traderId);
        if (trader?.SubscriptionPlan == null)
            return false;

        var plan = trader.SubscriptionPlan;
        if (plan.HasUnlimitedStrategies())
            return true;

        var activeStrategies = trader.Portfolios
            .SelectMany(p => p.Strategies)
            .Count(s => s.IsActive);

        return activeStrategies < plan.MaxActiveStrategies;
    }

    public async Task<bool> CanAddAssetAsync(string traderId)
    {
        var trader = await _traderRepository.GetByIdIncludePlanAndAssetsAsync(traderId);
        if (trader?.SubscriptionPlan == null)
            return false;

        var plan = trader.SubscriptionPlan;
        if (plan.HasUnlimitedAssets())
            return true;

        var activeAssets = trader.Portfolios
            .SelectMany(p => p.PortfolioAssets)
            .Count();

        return activeAssets < plan.MaxActiveAssets;
    }

    public async Task<bool> CanModifyBalanceAsync(string traderId)
    {
        var trader = await _traderRepository.GetByIdIncludeSubPlanAsync(traderId);
        return trader?.SubscriptionPlan?.CanModifyVirtualBalance ?? false;
    }

    public async Task<bool> CanSwitchToPlanAsync(string traderId, SubscriptionPlan newPlan)
    {
        var trader = await _traderRepository.GetByIdIncludePlanAndStrategyAsync(traderId);
        if (trader == null)
            return false;

        if (!newPlan.HasUnlimitedStrategies())
        {
            var activeStrategies = trader.Portfolios
                .SelectMany(p => p.Strategies)
                .Count(s => s.IsActive);

            if (activeStrategies > newPlan.MaxActiveStrategies)
                return false;
        }

        if (!newPlan.HasUnlimitedAssets())
        {
            var traderWithAssets = await _traderRepository.GetByIdIncludePlanAndAssetsAsync(traderId);
            if (traderWithAssets != null)
            {
                var activeAssets = traderWithAssets.Portfolios
                    .SelectMany(p => p.PortfolioAssets)
                    .Count();

                if (activeAssets > newPlan.MaxActiveAssets)
                    return false;
            }
        }

        return true;
    }
}
