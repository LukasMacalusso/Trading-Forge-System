namespace TraderForge.Domain.Entities;

public class Trader
{
    public string Id { get; private set; }
    public string UserName { get; set; }
    public string Email { get; private set; }
    
    public DateTime FreeTrialExpirationDate{ get; set; }
    public DateTime FreeTrialRegistrationDate { get; set; }
    
    public Guid? SubscriptionPlanId { get; private set; }
    public SubscriptionPlan? SubscriptionPlan { get; private set; }

    public ICollection<Portfolio> Portfolios { get; private set; } = new List<Portfolio>();

    public Trader(string id, string email)
    {
        Id = id;
        Email = email;
    }


    public void ChangeSubscriptionPlan(SubscriptionPlan newPlan)
    {
        AssignSubscriptionPlan(newPlan);
        FreezeActivePortfolio();
        Portfolios.Add(new Portfolio(Id, newPlan.InitialVirtualBalance));
        
    }
    
    public void AssignSubscriptionPlan(SubscriptionPlan plan)
    {
        SubscriptionPlanId = plan.Id;
        //SubscriptionPlan = plan;
    }

    private void FreezeActivePortfolio()
    {
        var activePortfolio = Portfolios.FirstOrDefault(p => p.IsActive);
        if (activePortfolio != null){activePortfolio.FreezeSimulation();}
    }

    public void ClearSubscriptionPlan()
    {
        SubscriptionPlanId = null;
        SubscriptionPlan = null;
    }

    public void ResetPortfolio()
    {
        if (SubscriptionPlan == null)
            throw new InvalidOperationException("No subscription plan assigned.");

        var activePortfolio = Portfolios.FirstOrDefault(p => p.IsActive);
        if (activePortfolio != null)
        {
            activePortfolio.FreezeSimulation();
            activePortfolio.AddFunds(0, "Reset", null, null, null, 0);
        }

        var newPortfolio = new Portfolio(Id, SubscriptionPlan.InitialVirtualBalance);
        newPortfolio.AddFunds(SubscriptionPlan.InitialVirtualBalance, "Reset", null, null, null, 0);
        Portfolios.Add(newPortfolio);
    }

    public void InitializeWithPlan(SubscriptionPlan plan)
    {
        AssignSubscriptionPlan(plan);
        var initialPortfolio = new Portfolio(Id, plan.InitialVirtualBalance);
        Portfolios.Add(initialPortfolio);
    }

    public void BuyPosition(string symbol, decimal quantity, decimal price, ICommissionService commissionService)
    {
        var activePortfolio = Portfolios.FirstOrDefault(p => p.IsActive);
        if (activePortfolio == null)
            throw new InvalidOperationException("No active portfolio found.");

        activePortfolio.BuyPosition(symbol, quantity, price, commissionService);

        if (!activePortfolio.IsActive)
        {
            ResetPortfolio();
        }
    }

    public void SellPosition(string symbol, decimal quantity, decimal price, ICommissionService commissionService)
    {
        var activePortfolio = Portfolios.FirstOrDefault(p => p.IsActive);
        if (activePortfolio == null)
            throw new InvalidOperationException("No active portfolio found.");

        activePortfolio.SellPosition(symbol, quantity, price, commissionService);
    }
}