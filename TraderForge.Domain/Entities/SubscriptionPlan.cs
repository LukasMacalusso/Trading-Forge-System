namespace TraderForge.Domain.Entities;

public class SubscriptionPlan
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public decimal MonthlyPrice { get; private set; }
    public decimal InitialVirtualBalance { get; private set; }
    
    public int? MaxActiveStrategies { get; private set; }
    public int? MaxActiveAssets { get; private set; }
    
    public bool CanModifyVirtualBalance { get; private set; }

    public ICollection<Trader> Traders { get; private set; } = new List<Trader>();
    
    private SubscriptionPlan(){}
    
    public SubscriptionPlan(Guid id, string name, decimal monthlyPrice, decimal initialVirtualBalance, 
        int? maxActiveStrategies, int? maxActiveAssets, bool canModifyVirtualBalance)
    {
        Id = id;
        Name = name;
        MonthlyPrice = monthlyPrice;
        InitialVirtualBalance = initialVirtualBalance;
        MaxActiveStrategies = maxActiveStrategies;
        MaxActiveAssets = maxActiveAssets;
        CanModifyVirtualBalance = canModifyVirtualBalance;
    }

    public void Update(string newName, 
        decimal newMonthlyPrice, 
        decimal newInitialVirtualBalance, 
        int? newMaxActiveStrategies,
        int? newMaxActiveAssets,
        bool newCanModifyVirtualBalance)
    {
        Name = newName;
        MonthlyPrice = newMonthlyPrice;
        InitialVirtualBalance = newInitialVirtualBalance;
        MaxActiveAssets = newMaxActiveAssets;
        MaxActiveStrategies = newMaxActiveStrategies;
        CanModifyVirtualBalance = newCanModifyVirtualBalance;
    }
    
    public bool HasUnlimitedStrategies() => MaxActiveStrategies == null;
    public bool HasUnlimitedAssets() => MaxActiveAssets == null;

}