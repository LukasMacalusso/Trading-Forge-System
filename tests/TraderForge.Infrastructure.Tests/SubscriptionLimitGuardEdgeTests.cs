using Moq;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Repositories;
using TraderForge.Infrastructure.Services;
using Xunit;

namespace TraderForge.Infrastructure.Tests;

public class SubscriptionLimitGuardEdgeTests
{
    private readonly Mock<ITraderRepository> _traderRepoMock;
    private readonly SubscriptionLimitGuard _guard;

    public SubscriptionLimitGuardEdgeTests()
    {
        _traderRepoMock = new Mock<ITraderRepository>();
        _guard = new SubscriptionLimitGuard(_traderRepoMock.Object);
    }

    [Fact]
    public async Task CanAddStrategyAsync_ExceedsLimit_ReturnsFalse()
    {
        var trader = CreateTraderWithPlan(maxStrats: 1, positionsCount: 0, activeStrategiesCount: 1);
        _traderRepoMock.Setup(x => x.GetByIdIncludePlanAndStrategyAsync("user-1")).ReturnsAsync(trader);

        var result = await _guard.CanAddStrategyAsync("user-1");

        Assert.False(result);
    }

    [Fact]
    public async Task CanAddAssetAsync_ExceedsLimit_ReturnsFalse()
    {
        var trader = CreateTraderWithPlan(maxStrats: null, maxAssets: 2, positionsCount: 2);
        _traderRepoMock.Setup(x => x.GetByIdIncludePlanAndPositionsAsync("user-1")).ReturnsAsync(trader);

        var result = await _guard.CanAddAssetAsync("user-1");

        Assert.False(result);
    }

    [Fact]
    public async Task CanModifyBalanceAsync_PlanDisallows_ReturnsFalse()
    {
        var trader = CreateTraderWithPlan(maxStrats: null, maxAssets: null, canModifyBalance: false);
        _traderRepoMock.Setup(x => x.GetByIdIncludeSubPlanAsync("user-1")).ReturnsAsync(trader);

        var result = await _guard.CanModifyBalanceAsync("user-1");

        Assert.False(result);
    }

    [Fact]
    public async Task CanSwitchToPlanAsync_ExceedsStrategyLimit_ReturnsFalse()
    {
        var trader = CreateTraderWithPlan(maxStrats: 1, maxAssets: null, positionsCount: 0, activeStrategiesCount: 2);
        _traderRepoMock.Setup(x => x.GetByIdIncludePlanAndStrategyAsync("user-1")).ReturnsAsync(trader);

        var newPlan = new SubscriptionPlan(Guid.NewGuid(), "Basic", 10, 10, 1, 5, false);
        var result = await _guard.CanSwitchToPlanAsync("user-1", newPlan);

        Assert.False(result);
    }

    [Fact]
    public async Task CanSwitchToPlanAsync_ExceedsAssetLimit_ReturnsFalse()
    {
        var trader = CreateTraderWithPlan(maxStrats: null, maxAssets: 5, positionsCount: 3, activeStrategiesCount: 0);
        _traderRepoMock.Setup(x => x.GetByIdIncludePlanAndStrategyAsync("user-1")).ReturnsAsync(trader);
        _traderRepoMock.Setup(x => x.GetByIdIncludePlanAndPositionsAsync("user-1")).ReturnsAsync(trader);

        var newPlan = new SubscriptionPlan(Guid.NewGuid(), "Basic", 10, 10, 5, 2, false);
        var result = await _guard.CanSwitchToPlanAsync("user-1", newPlan);

        Assert.False(result);
    }

    private static Trader CreateTraderWithPlan(int? maxStrats = null, int? maxAssets = null, bool canModifyBalance = false, int positionsCount = 0, int activeStrategiesCount = 0)
    {
        var trader = new Trader("user-1", "test@test.com");
        var planId = Guid.NewGuid();
        var plan = new SubscriptionPlan(planId, "Pro", 29.99m, 50000m, maxStrats, maxAssets, canModifyBalance);
        var activeSub = new ActiveSubscription("user-1", planId, 30);
        typeof(ActiveSubscription).GetProperty("Plan")!.SetValue(activeSub, plan);
        typeof(Trader).GetProperty("Subscription")!.SetValue(trader, activeSub);

        var portfolio = new Portfolio("user-1", 10000m);

        for (int i = 0; i < activeStrategiesCount; i++)
        {
            var strategy = new Strategy(Guid.NewGuid(), $"S{i}", portfolio.Id);
            typeof(Strategy).GetProperty("IsActive")!.SetValue(strategy, true);
            portfolio.Strategies.Add(strategy);
        }

        for (int i = 0; i < positionsCount; i++)
        {
            portfolio.Positions.Add(new Position(Guid.NewGuid(), $"ASSET{i}", 10m, 100m, portfolio.Id));
        }

        trader.Portfolios.Add(portfolio);
        return trader;
    }
}
