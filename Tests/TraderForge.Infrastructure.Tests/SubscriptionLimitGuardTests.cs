using Moq;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Repositories;
using TraderForge.Infrastructure.Services;

namespace TraderForge.Infrastructure.Tests;

public class SubscriptionLimitGuardTests
{
    private readonly Mock<ITraderRepository> _traderRepoMock;
    private readonly SubscriptionLimitGuard _guard;

    public SubscriptionLimitGuardTests()
    {
        _traderRepoMock = new Mock<ITraderRepository>();
        _guard = new SubscriptionLimitGuard(_traderRepoMock.Object);
    }

    [Fact]
    public async Task CanAddStrategyAsync_PlanNotFound_ReturnsFalse()
    {
        _traderRepoMock.Setup(x => x.GetByIdIncludePlanAndStrategyAsync(It.IsAny<string>()))
            .ReturnsAsync((Trader?)null);

        var result = await _guard.CanAddStrategyAsync("user-1");
        Assert.False(result);
    }

    [Fact]
    public async Task CanAddStrategyAsync_UnlimitedStrategies_ReturnsTrue()
    {
        var trader = CreateTraderWithPlan(unlimitedStrategies: true, maxStrats: null, maxAssets: null);
        _traderRepoMock.Setup(x => x.GetByIdIncludePlanAndStrategyAsync("user-1")).ReturnsAsync(trader);

        var result = await _guard.CanAddStrategyAsync("user-1");
        Assert.True(result);
    }

    [Fact]
    public async Task CanAddStrategyAsync_WithinLimit_ReturnsTrue()
    {
        var trader = CreateTraderWithPlan(unlimitedStrategies: false, maxStrats: 2, maxAssets: null);
        _traderRepoMock.Setup(x => x.GetByIdIncludePlanAndStrategyAsync("user-1")).ReturnsAsync(trader);

        var result = await _guard.CanAddStrategyAsync("user-1");
        Assert.True(result);
    }

    [Fact]
    public async Task CanAddAssetAsync_PlanNotFound_ReturnsFalse()
    {
        _traderRepoMock.Setup(x => x.GetByIdIncludePlanAndPositionsAsync(It.IsAny<string>()))
            .ReturnsAsync((Trader?)null);

        var result = await _guard.CanAddAssetAsync("user-1");
        Assert.False(result);
    }

    [Fact]
    public async Task CanAddAssetAsync_UnlimitedAssets_ReturnsTrue()
    {
        var trader = CreateTraderWithPlan(unlimitedStrategies: true, maxStrats: null, maxAssets: null);
        _traderRepoMock.Setup(x => x.GetByIdIncludePlanAndPositionsAsync("user-1")).ReturnsAsync(trader);

        var result = await _guard.CanAddAssetAsync("user-1");
        Assert.True(result);
    }

    [Fact]
    public async Task CanAddAssetAsync_WithinLimit_ReturnsTrue()
    {
        var trader = CreateTraderWithPlan(unlimitedStrategies: false, maxStrats: null, maxAssets: 5);
        _traderRepoMock.Setup(x => x.GetByIdIncludePlanAndPositionsAsync("user-1")).ReturnsAsync(trader);

        var result = await _guard.CanAddAssetAsync("user-1");
        Assert.True(result);
    }

    [Fact]
    public async Task CanModifyBalanceAsync_PlanNotFound_ReturnsFalse()
    {
        _traderRepoMock.Setup(x => x.GetByIdIncludeSubPlanAsync(It.IsAny<string>()))
            .ReturnsAsync((Trader?)null);

        var result = await _guard.CanModifyBalanceAsync("user-1");
        Assert.False(result);
    }

    [Fact]
    public async Task CanModifyBalanceAsync_PlanAllows_ReturnsTrue()
    {
        var trader = CreateTraderWithPlan(unlimitedStrategies: false, maxStrats: null, maxAssets: null, canModifyBalance: true);
        _traderRepoMock.Setup(x => x.GetByIdIncludeSubPlanAsync("user-1")).ReturnsAsync(trader);

        var result = await _guard.CanModifyBalanceAsync("user-1");
        Assert.True(result);
    }

    [Fact]
    public async Task CanSwitchToPlanAsync_TraderNotFound_ReturnsFalse()
    {
        _traderRepoMock.Setup(x => x.GetByIdIncludePlanAndStrategyAsync(It.IsAny<string>()))
            .ReturnsAsync((Trader?)null);

        var result = await _guard.CanSwitchToPlanAsync("user-1", new SubscriptionPlan(Guid.NewGuid(), "Pro", 10, 10, 1, 1, false));
        Assert.False(result);
    }

    [Fact]
    public async Task CanSwitchToPlanAsync_WithinLimits_ReturnsTrue()
    {
        var trader = CreateTraderWithPlan(unlimitedStrategies: false, maxStrats: 1, maxAssets: 1);
        _traderRepoMock.Setup(x => x.GetByIdIncludePlanAndStrategyAsync("user-1")).ReturnsAsync(trader);
        _traderRepoMock.Setup(x => x.GetByIdIncludePlanAndPositionsAsync("user-1")).ReturnsAsync(trader);

        var newPlan = new SubscriptionPlan(Guid.NewGuid(), "Pro", 10, 10, 2, 2, false);

        var result = await _guard.CanSwitchToPlanAsync("user-1", newPlan);
        Assert.True(result);
    }

    private Trader CreateTraderWithPlan(bool unlimitedStrategies, int? maxStrats, int? maxAssets, bool canModifyBalance = false)
    {
        var trader = new Trader("user-1", "test@test.com");
        var planId = Guid.NewGuid();
        var plan = new SubscriptionPlan(planId, "Pro", 29.99m, 50000m, maxStrats, maxAssets, canModifyBalance);
        var activeSub = new ActiveSubscription("user-1", planId, 30);
        typeof(ActiveSubscription).GetProperty("Plan")!.SetValue(activeSub, plan);
        typeof(Trader).GetProperty("Subscription")!.SetValue(trader, activeSub);
        return trader;
    }
}
