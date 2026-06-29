using TraderForge.Domain.Services;

namespace TraderForge.Domain.Entities;

public class Trader
{
    public string Id { get; private set; } = null!;
    public string UserName { get; set; } = null!;
    public string Email { get; private set; } = null!;
    public bool IsSuspended { get; private set; } = false;
    public string SuspensionReason { get; private set; } = "";
    public ActiveSubscription? Subscription { get; private set; }

    public ICollection<Portfolio> Portfolios { get; private set; } = new List<Portfolio>();

    public Trader(string id, string email)
    {
        Id = id;
        Email = email;
        UserName = email;
    }


    public void ProcessPayment(SubscriptionPlan plan)
    {
        if (Subscription == null)
        {
            Subscription = new ActiveSubscription(Id, plan.Id, 30);
            ResetPortfolio(plan.InitialVirtualBalance);
            return;
        }

        if (Subscription.SubscriptionPlanId == plan.Id)
        {
            Subscription.Extend(30);
        }
        else
        {
            Subscription.ChangePlan(plan.Id);
            ResetPortfolio(plan.InitialVirtualBalance);
        }
    }

    public void ResetPortfolio(decimal initialVirtualBalance)
    {
        var activePortfolio = Portfolios.FirstOrDefault(p => p.IsActive);
        if (activePortfolio != null)
        {
            activePortfolio.FreezeSimulation();
        }

        var newPortfolio = new Portfolio(Id, initialVirtualBalance);
        Portfolios.Add(newPortfolio);
    }

    public void InitializeWithTrial(SubscriptionPlan basicPlan)
    {
        Subscription = new ActiveSubscription(Id, basicPlan.Id, 7);
        var initialPortfolio = new Portfolio(Id, basicPlan.InitialVirtualBalance);
        Portfolios.Add(initialPortfolio);
    }
    
    public void CancelSubscription()
    {
        Subscription?.Cancel();
    }

    public void BuyPosition(string symbol, decimal quantity, decimal price, ICommissionService commissionService)
    {
        var activePortfolio = Portfolios.FirstOrDefault(p => p.IsActive);
        if (activePortfolio == null)
            throw new InvalidOperationException("No active portfolio found.");

        activePortfolio.BuyPosition(symbol, quantity, price, commissionService);

        if (!activePortfolio.IsActive)
        {
            // If bankruptcy occurred, reset portfolio based on their current plan limits?
            // This would require injecting the plan balance, but they don't have a plan reference directly accessible here without navigation prop loading.
            // For now, this is a placeholder. 
        }
    }

    public void SellPosition(string symbol, decimal quantity, decimal price, ICommissionService commissionService)
    {
        var activePortfolio = Portfolios.FirstOrDefault(p => p.IsActive);
        if (activePortfolio == null)
            throw new InvalidOperationException("No active portfolio found.");

        activePortfolio.SellPosition(symbol, quantity, price, commissionService);
    }
    
    public void Suspend(string reason)
    {
        IsSuspended = true;
        SuspensionReason = reason;
    }
    
    public void Unsuspend()
    {
        IsSuspended = false;
        SuspensionReason = string.Empty;
    }
}