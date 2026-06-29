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
        Assert.Null(trader.Subscription);
    }

    [Fact]
    public void InitializeWithTrial_ShouldSetActiveSubscriptionAndPortfolio()
    {
        var trader = new Trader(_traderId, Email);
        var plan = new SubscriptionPlan(Guid.NewGuid(), "Basic", 9.99m, 10000m, 2, 5, false);

        trader.InitializeWithTrial(plan);

        Assert.NotNull(trader.Subscription);
        Assert.Equal(plan.Id, trader.Subscription.SubscriptionPlanId);
        Assert.Single(trader.Portfolios);
        Assert.Equal(10000m, trader.Portfolios.First().VirtualBalance);
    }

    [Fact]
    public void ProcessPayment_WhenNoSubscription_ShouldCreateSubscriptionAndPortfolio()
    {
        var trader = new Trader(_traderId, Email);
        var plan = new SubscriptionPlan(Guid.NewGuid(), "Pro", 29.99m, 50000m, 10, 20, false);

        trader.ProcessPayment(plan);

        Assert.NotNull(trader.Subscription);
        Assert.Equal(plan.Id, trader.Subscription.SubscriptionPlanId);
        Assert.Single(trader.Portfolios);
        Assert.Equal(50000m, trader.Portfolios.First().VirtualBalance);
    }

    [Fact]
    public void ProcessPayment_WhenSamePlan_ShouldExtendEndDate()
    {
        var trader = new Trader(_traderId, Email);
        var plan = new SubscriptionPlan(Guid.NewGuid(), "Basic", 9.99m, 10000m, 2, 5, false);
        
        trader.InitializeWithTrial(plan);
        var originalEndDate = trader.Subscription!.EndDate;

        trader.ProcessPayment(plan);

        Assert.Equal(plan.Id, trader.Subscription.SubscriptionPlanId);
        Assert.True(trader.Subscription.EndDate > originalEndDate);
        Assert.Single(trader.Portfolios); // Portfolio should NOT be reset
    }

    [Fact]
    public void ProcessPayment_WhenDifferentPlan_ShouldChangePlanAndResetPortfolio()
    {
        var trader = new Trader(_traderId, Email);
        var oldPlan = new SubscriptionPlan(Guid.NewGuid(), "Basic", 9.99m, 10000m, 2, 5, false);
        var newPlan = new SubscriptionPlan(Guid.NewGuid(), "Pro", 29.99m, 50000m, 10, 20, false);

        trader.InitializeWithTrial(oldPlan);
        var oldPortfolio = trader.Portfolios.First();

        trader.ProcessPayment(newPlan);

        Assert.Equal(newPlan.Id, trader.Subscription!.SubscriptionPlanId);
        Assert.False(oldPortfolio.IsActive);
        Assert.Equal(2, trader.Portfolios.Count);
        var newPortfolio = trader.Portfolios.First(p => p.IsActive);
        Assert.Equal(newPlan.InitialVirtualBalance, newPortfolio.VirtualBalance);
    }

    [Fact]
    public void Suspend_WhenCalled_SetsIsSuspendedAndReason()
    {
        var trader = new Trader(_traderId, Email);
        var reason = "Violation of terms";

        trader.Suspend(reason);

        Assert.True(trader.IsSuspended);
        Assert.Equal(reason, trader.SuspensionReason);
    }

    [Fact]
    public void Unsuspend_WhenCalled_SetsIsSuspendedToFalseAndClearsReason()
    {
        var trader = new Trader(_traderId, Email);
        trader.Suspend("Violation");
        
        trader.Unsuspend();

        Assert.False(trader.IsSuspended);
        Assert.Equal(string.Empty, trader.SuspensionReason);
    }
}
