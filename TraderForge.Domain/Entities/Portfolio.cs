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
    public ICollection<Position> Positions { get; private set; } = new List<Position>();
    public ICollection<Transaction> Transactions { get; private set; } = new List<Transaction>();
    public ICollection<Order> Orders { get; private set; } = new List<Order>();

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

    public void DeductFunds(decimal total, string type, string? symbol, decimal? qty, decimal? price, decimal commission)
    {
        var balanceBefore = VirtualBalance;
        VirtualBalance -= total;

        Transactions.Add(new Transaction(
            Id, type, total, balanceBefore, VirtualBalance, commission, symbol, qty, price));

        if (VirtualBalance <= 0)
            FreezeSimulation();
    }

    public void AddFunds(decimal total, string type, string? symbol, decimal? qty, decimal? price, decimal commission)
    {
        var balanceBefore = VirtualBalance;
        VirtualBalance += total;

        Transactions.Add(new Transaction(
            Id, type, total, balanceBefore, VirtualBalance, commission, symbol, qty, price));
    }
}
