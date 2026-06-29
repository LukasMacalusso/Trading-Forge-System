namespace TraderForge.Domain.Entities;

public class ActiveSubscription
{
    public Guid Id { get; private set; }
    public string TraderId { get; private set; } = null!;
    public Trader Trader { get; private set; } = null!;

    public Guid SubscriptionPlanId { get; private set; }
    public SubscriptionPlan Plan { get; private set; } = null!;

    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }

    public bool IsActive => DateTime.UtcNow <= EndDate;

    private ActiveSubscription() { } // For EF Core

    public ActiveSubscription(string traderId, Guid subscriptionPlanId, int initialDaysValid = 7)
    {
        Id = Guid.NewGuid();
        TraderId = traderId;
        SubscriptionPlanId = subscriptionPlanId;
        StartDate = DateTime.UtcNow;
        EndDate = DateTime.UtcNow.AddDays(initialDaysValid);
    }

    public bool IsWithinTrialPeriod(int trialDays = 7)
    {
        return (DateTime.UtcNow - StartDate).TotalDays <= trialDays;
    }

    public void ChangePlan(Guid newPlanId)
    {
        SubscriptionPlanId = newPlanId;
        StartDate = DateTime.UtcNow;
        EndDate = DateTime.UtcNow.AddDays(30);
    }

    public void Extend(int days)
    {
        EndDate = EndDate.AddDays(days);
    }
    
    public void Cancel()
    {
        EndDate = DateTime.UtcNow;
    }
}
