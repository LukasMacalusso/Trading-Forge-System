using TraderForge.Domain.Entities;

namespace TraderForge.Domain.Tests;

public class SubscriptionPlanTests
{
    private readonly Guid _planId = Guid.NewGuid();

    [Fact]
    public void Constructor_ShouldSetProperties()
    {
        var plan = new SubscriptionPlan(_planId, "Basic", 9.99m, 10000m, 2, 5, false);

        Assert.Equal(_planId, plan.Id);
        Assert.Equal("Basic", plan.Name);
        Assert.Equal(9.99m, plan.MonthlyPrice);
        Assert.Equal(10000m, plan.InitialVirtualBalance);
        Assert.Equal(2, plan.MaxActiveStrategies);
        Assert.Equal(5, plan.MaxActiveAssets);
        Assert.False(plan.CanModifyVirtualBalance);
    }

    [Fact]
    public void Update_ShouldModifyAllProperties()
    {
        var plan = new SubscriptionPlan(_planId, "Basic", 9.99m, 10000m, 2, 5, false);

        plan.Update("Basic Updated", 14.99m, 20000m, 5, 10, true);

        Assert.Equal("Basic Updated", plan.Name);
        Assert.Equal(14.99m, plan.MonthlyPrice);
        Assert.Equal(20000m, plan.InitialVirtualBalance);
        Assert.Equal(5, plan.MaxActiveStrategies);
        Assert.Equal(10, plan.MaxActiveAssets);
        Assert.True(plan.CanModifyVirtualBalance);
    }

    [Fact]
    public void HasUnlimitedStrategies_WhenMaxIsNull_ReturnsTrue()
    {
        var plan = new SubscriptionPlan(_planId, "Unlimited", 49.99m, 100000m, null, null, true);

        Assert.True(plan.HasUnlimitedStrategies());
        Assert.True(plan.HasUnlimitedAssets());
    }

    [Fact]
    public void HasUnlimitedStrategies_WhenMaxIsSet_ReturnsFalse()
    {
        var plan = new SubscriptionPlan(_planId, "Basic", 9.99m, 10000m, 2, 5, false);

        Assert.False(plan.HasUnlimitedStrategies());
        Assert.False(plan.HasUnlimitedAssets());
    }
}
