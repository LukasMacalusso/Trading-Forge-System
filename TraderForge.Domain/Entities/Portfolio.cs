using System.Text.Json.Serialization;

namespace TraderForge.Domain.Entities;
public class Portfolio
{
    public Guid Id { get; private set; }
    public decimal VirtualBalance { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ClosedAt { get; private set; }

    
    public ICollection<Strategy> Strategies { get; private set; } = new List<Strategy>();
    public ICollection<PortfolioAsset> PortfolioAssets { get; private set; } = new List<PortfolioAsset>();

    public string TraderId { get; private set; }
    [JsonIgnore]
    public Trader Trader { get; private set; } = null!;

    private Portfolio(){}

    public Portfolio(string traderId, decimal initialBalance)
    {
        Id = Guid.NewGuid();
        TraderId = traderId;
        VirtualBalance = initialBalance;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void FreezeSimulation()
    {
        IsActive = false;
        ClosedAt = DateTime.UtcNow;
    }
}
