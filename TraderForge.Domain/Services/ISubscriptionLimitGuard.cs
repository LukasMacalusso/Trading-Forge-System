using TraderForge.Domain.Entities;

namespace TraderForge.Domain.Services;

public interface ISubscriptionLimitGuard
{
    Task<bool> CanAddStrategyAsync(string traderId);
    Task<bool> CanAddAssetAsync(string traderId);
    Task<bool> CanModifyBalanceAsync(string traderId);
    Task<bool> CanSwitchToPlanAsync(string traderId, SubscriptionPlan newPlan);
}
