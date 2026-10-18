namespace TraderForge.Domain.Entities;

public class Transaction
{
    public Guid Id { get; private set; }
    public Guid PortfolioId { get; private set; }
    public Portfolio Portfolio { get; private set; } = null!;
    public string Type { get; private set; } = null!;
    public string? Symbol { get; private set; }
    public decimal? Quantity { get; private set; }
    public decimal? Price { get; private set; }
    public decimal Commission { get; private set; }
    public decimal Total { get; private set; }
    public decimal BalanceBefore { get; private set; }
    public decimal BalanceAfter { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Transaction() { }

    public Transaction(
        Guid portfolioId,
        string type,
        decimal total,
        decimal balanceBefore,
        decimal balanceAfter,
        decimal commission,
        string? symbol = null,
        decimal? quantity = null,
        decimal? price = null)
    {
        Id = Guid.NewGuid();
        PortfolioId = portfolioId;
        Type = type;
        Total = total;
        BalanceBefore = balanceBefore;
        BalanceAfter = balanceAfter;
        Commission = commission;
        Symbol = symbol;
        Quantity = quantity;
        Price = price;
        CreatedAt = DateTime.UtcNow;
    }
}
