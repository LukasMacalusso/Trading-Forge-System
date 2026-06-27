using TraderForge.Domain.Entities;

namespace TraderForge.Domain.Tests;

public class ActiveSubscriptionTests
{
    [Fact]
    public void Constructor_ShouldSetProperties()
    {
        var traderId = "user-1";
        var planId = Guid.NewGuid();
        var sub = new ActiveSubscription(traderId, planId, 7);

        Assert.Equal(traderId, sub.TraderId);
        Assert.Equal(planId, sub.SubscriptionPlanId);
        Assert.True(sub.StartDate <= DateTime.UtcNow);
        Assert.True(sub.EndDate > sub.StartDate);
        Assert.True(sub.IsActive);
    }

    [Fact]
    public void ChangePlan_ShouldUpdatePlanIdAndExtendEndDateBy30Days()
    {
        var traderId = "user-1";
        var oldPlanId = Guid.NewGuid();
        var sub = new ActiveSubscription(traderId, oldPlanId, 7);

        var newPlanId = Guid.NewGuid();
        sub.ChangePlan(newPlanId);

        Assert.Equal(newPlanId, sub.SubscriptionPlanId);
        Assert.True(sub.IsActive);
        var expectedEnd = DateTime.UtcNow.AddDays(30);
        Assert.True(sub.EndDate > DateTime.UtcNow.AddDays(29) && sub.EndDate <= expectedEnd);
    }

    [Fact]
    public void Extend_ShouldAddDaysToEndDate()
    {
        var sub = new ActiveSubscription("user-1", Guid.NewGuid(), 10);
        var originalEndDate = sub.EndDate;

        sub.Extend(15);

        Assert.Equal(originalEndDate.AddDays(15), sub.EndDate);
    }

    [Fact]
    public void IsActive_ShouldBeFalse_WhenEndDateIsPast()
    {
        var sub = new ActiveSubscription("user-1", Guid.NewGuid(), 10);
        
        typeof(ActiveSubscription).GetProperty("EndDate")!.SetValue(sub, DateTime.UtcNow.AddDays(-1));

        Assert.False(sub.IsActive);
    }
}
