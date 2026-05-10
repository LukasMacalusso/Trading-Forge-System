using TraderForge.Domain.Entities;

namespace TraderForge.Domain.Tests;

public class TraderTests
{
    private readonly string _traderId = Guid.NewGuid().ToString();
    private const string Email = "test@traderforge.com";

    [Fact]
    public void Constructor_ShouldSetProperties()
    {
        var trader = new Trader(_traderId, Email);

        Assert.Equal(_traderId, trader.Id);
        Assert.Equal(Email, trader.Email);
        Assert.Empty(trader.Portfolios);
        Assert.Null(trader.SubscriptionPlanId);
    }

    [Fact]
    public void AssignSubscriptionPlan_ShouldSetPlanId()
    {
        var trader = new Trader(_traderId, Email);
        var plan = new SubscriptionPlan(Guid.NewGuid(), "Basic", 9.99m, 10000m, 2, 5, false);

        trader.AssignSubscriptionPlan(plan);

        Assert.Equal(plan.Id, trader.SubscriptionPlanId);
    }

    [Fact]
    public void ChangeSubscriptionPlan_ShouldFreezeOldPortfolioAndAddNewOne()
    {
        var trader = new Trader(_traderId, Email);
        var oldPlan = new SubscriptionPlan(Guid.NewGuid(), "Basic", 9.99m, 10000m, 2, 5, false);
        var newPlan = new SubscriptionPlan(Guid.NewGuid(), "Pro", 29.99m, 50000m, 10, 20, false);

        trader.AssignSubscriptionPlan(oldPlan);
        var oldPortfolio = new Portfolio(_traderId, 10000m);
        trader.Portfolios.Add(oldPortfolio);

        trader.ChangeSubscriptionPlan(newPlan);

        Assert.Equal(newPlan.Id, trader.SubscriptionPlanId);
        Assert.False(oldPortfolio.IsActive);
        Assert.NotNull(oldPortfolio.ClosedAt);
        Assert.Equal(2, trader.Portfolios.Count);
        var newPortfolio = trader.Portfolios.First(p => p.IsActive);
        Assert.Equal(newPlan.InitialVirtualBalance, newPortfolio.VirtualBalance);
    }

    [Fact]
    public void ClearSubscriptionPlan_ShouldSetPlanIdToNull()
    {
        var trader = new Trader(_traderId, Email);
        var plan = new SubscriptionPlan(Guid.NewGuid(), "Basic", 9.99m, 10000m, 2, 5, false);
        trader.AssignSubscriptionPlan(plan);

        trader.ClearSubscriptionPlan();

        Assert.Null(trader.SubscriptionPlanId);
        Assert.Null(trader.SubscriptionPlan);
    }

    [Fact]
    public void ChangeSubscriptionPlan_WhenNoActivePortfolio_ShouldStillAddNewPortfolio()
    {
        var trader = new Trader(_traderId, Email);
        var plan = new SubscriptionPlan(Guid.NewGuid(), "Pro", 29.99m, 50000m, 10, 20, false);

        trader.ChangeSubscriptionPlan(plan);

        Assert.Single(trader.Portfolios);
        Assert.True(trader.Portfolios.First().IsActive);
    }
}
