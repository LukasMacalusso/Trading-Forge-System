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

    private static void SetSubscriptionPlan(Trader trader, SubscriptionPlan plan)
    {
        trader.AssignSubscriptionPlan(plan);
        var prop = typeof(Trader).GetProperty("SubscriptionPlan",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        prop!.SetValue(trader, plan);
    }

    [Fact]
    public async Task CanAddStrategyAsync_WhenUnlimitedStrategies_ReturnsTrue()
    {
        var traderId = Guid.NewGuid().ToString();
        var plan = new SubscriptionPlan(Guid.NewGuid(), "Unlimited", 49.99m, 100000m, null, null, true);
        var trader = new Trader(traderId, "test@test.com");
        SetSubscriptionPlan(trader, plan);

        _traderRepoMock.Setup(x => x.GetByIdIncludePlanAndStrategyAsync(traderId)).ReturnsAsync(trader);

        var result = await _guard.CanAddStrategyAsync(traderId);
        Assert.True(result);
    }

    [Fact]
    public async Task CanAddStrategyAsync_WhenUnderLimit_ReturnsTrue()
    {
        var traderId = Guid.NewGuid().ToString();
        var plan = new SubscriptionPlan(Guid.NewGuid(), "Basic", 9.99m, 10000m, 2, 5, false);
        var trader = new Trader(traderId, "test@test.com");
        SetSubscriptionPlan(trader, plan);
        var portfolio = new Portfolio(traderId, 10000m);
        portfolio.Strategies.Add(new Strategy(Guid.NewGuid(), "S1", portfolio.Id));
        trader.Portfolios.Add(portfolio);

        _traderRepoMock.Setup(x => x.GetByIdIncludePlanAndStrategyAsync(traderId)).ReturnsAsync(trader);

        var result = await _guard.CanAddStrategyAsync(traderId);
        Assert.True(result);
    }

    [Fact]
    public async Task CanAddStrategyAsync_WhenAtLimit_ReturnsFalse()
    {
        var traderId = Guid.NewGuid().ToString();
        var plan = new SubscriptionPlan(Guid.NewGuid(), "Basic", 9.99m, 10000m, 1, 5, false);
        var trader = new Trader(traderId, "test@test.com");
        SetSubscriptionPlan(trader, plan);
        var portfolio = new Portfolio(traderId, 10000m);
        var strategy = new Strategy(Guid.NewGuid(), "S1", portfolio.Id);
        portfolio.Strategies.Add(strategy);
        trader.Portfolios.Add(portfolio);

        _traderRepoMock.Setup(x => x.GetByIdIncludePlanAndStrategyAsync(traderId)).ReturnsAsync(trader);

        var result = await _guard.CanAddStrategyAsync(traderId);
        Assert.False(result);
    }

    [Fact]
    public async Task CanAddStrategyAsync_WhenNoPlan_ReturnsFalse()
    {
        var traderId = Guid.NewGuid().ToString();
        var trader = new Trader(traderId, "test@test.com");

        _traderRepoMock.Setup(x => x.GetByIdIncludePlanAndStrategyAsync(traderId)).ReturnsAsync(trader);

        var result = await _guard.CanAddStrategyAsync(traderId);
        Assert.False(result);
    }

    [Fact]
    public async Task CanAddAssetAsync_WhenUnlimitedAssets_ReturnsTrue()
    {
        var traderId = Guid.NewGuid().ToString();
        var plan = new SubscriptionPlan(Guid.NewGuid(), "Unlimited", 49.99m, 100000m, null, null, true);
        var trader = new Trader(traderId, "test@test.com");
        SetSubscriptionPlan(trader, plan);

        _traderRepoMock.Setup(x => x.GetByIdIncludePlanAndAssetsAsync(traderId)).ReturnsAsync(trader);

        var result = await _guard.CanAddAssetAsync(traderId);
        Assert.True(result);
    }

    [Fact]
    public async Task CanAddAssetAsync_WhenUnderLimit_ReturnsTrue()
    {
        var traderId = Guid.NewGuid().ToString();
        var plan = new SubscriptionPlan(Guid.NewGuid(), "Basic", 9.99m, 10000m, 2, 5, false);
        var trader = new Trader(traderId, "test@test.com");
        SetSubscriptionPlan(trader, plan);

        _traderRepoMock.Setup(x => x.GetByIdIncludePlanAndAssetsAsync(traderId)).ReturnsAsync(trader);

        var result = await _guard.CanAddAssetAsync(traderId);
        Assert.True(result);
    }

    [Fact]
    public async Task CanAddAssetAsync_WhenAtLimit_ReturnsFalse()
    {
        var traderId = Guid.NewGuid().ToString();
        var plan = new SubscriptionPlan(Guid.NewGuid(), "Basic", 9.99m, 10000m, 2, 1, false);
        var trader = new Trader(traderId, "test@test.com");
        SetSubscriptionPlan(trader, plan);
        var portfolio = new Portfolio(traderId, 10000m);
        portfolio.PortfolioAssets.Add(new PortfolioAsset(Guid.NewGuid(), "BTCUSDT", 1m, 50000m, portfolio.Id));
        trader.Portfolios.Add(portfolio);

        _traderRepoMock.Setup(x => x.GetByIdIncludePlanAndAssetsAsync(traderId)).ReturnsAsync(trader);

        var result = await _guard.CanAddAssetAsync(traderId);
        Assert.False(result);
    }

    [Fact]
    public async Task CanModifyBalanceAsync_WhenPlanAllows_ReturnsTrue()
    {
        var traderId = Guid.NewGuid().ToString();
        var plan = new SubscriptionPlan(Guid.NewGuid(), "Pro", 29.99m, 50000m, 10, 20, true);
        var trader = new Trader(traderId, "test@test.com");
        SetSubscriptionPlan(trader, plan);

        _traderRepoMock.Setup(x => x.GetByIdIncludeSubPlanAsync(traderId)).ReturnsAsync(trader);

        var result = await _guard.CanModifyBalanceAsync(traderId);
        Assert.True(result);
    }

    [Fact]
    public async Task CanModifyBalanceAsync_WhenPlanDisallows_ReturnsFalse()
    {
        var traderId = Guid.NewGuid().ToString();
        var plan = new SubscriptionPlan(Guid.NewGuid(), "Basic", 9.99m, 10000m, 2, 5, false);
        var trader = new Trader(traderId, "test@test.com");
        SetSubscriptionPlan(trader, plan);

        _traderRepoMock.Setup(x => x.GetByIdIncludeSubPlanAsync(traderId)).ReturnsAsync(trader);

        var result = await _guard.CanModifyBalanceAsync(traderId);
        Assert.False(result);
    }

    [Fact]
    public async Task CanSwitchToPlanAsync_WhenWithinLimits_ReturnsTrue()
    {
        var traderId = Guid.NewGuid().ToString();
        var newPlan = new SubscriptionPlan(Guid.NewGuid(), "Pro", 29.99m, 50000m, 10, 20, false);
        var trader = new Trader(traderId, "test@test.com");
        SetSubscriptionPlan(trader, newPlan);

        _traderRepoMock.Setup(x => x.GetByIdIncludePlanAndStrategyAsync(traderId)).ReturnsAsync(trader);
        _traderRepoMock.Setup(x => x.GetByIdIncludePlanAndAssetsAsync(traderId)).ReturnsAsync(trader);

        var result = await _guard.CanSwitchToPlanAsync(traderId, newPlan);
        Assert.True(result);
    }

    [Fact]
    public async Task CanSwitchToPlanAsync_WhenTraderNotFound_ReturnsFalse()
    {
        _traderRepoMock.Setup(x => x.GetByIdIncludePlanAndStrategyAsync(It.IsAny<string>()))
            .ReturnsAsync((Trader?)null!);

        var result = await _guard.CanSwitchToPlanAsync("nonexistent",
            new SubscriptionPlan(Guid.NewGuid(), "Pro", 29.99m, 50000m, 10, 20, false));
        Assert.False(result);
    }
}
