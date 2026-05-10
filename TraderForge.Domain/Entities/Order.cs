namespace TraderForge.Domain.Entities;

public class Order
{
    public Guid Id { get; private set; }
    public Guid PortfolioId { get; private set; }
    public Portfolio Portfolio { get; private set; } = null!;
    
    public string Symbol { get; private set; }
    public string Side { get; private set; } // "Buy" or "Sell"
    public string Type { get; private set; } // "Market" or "Limit"
    public decimal Quantity { get; private set; }
    public decimal Price { get; private set; }
    public decimal Commission { get; private set; }
    public decimal Total { get; private set; }
    public string Status { get; private set; } // "Pending", "Filled", "Cancelled"
    
    public DateTime CreatedAt { get; private set; }
    public DateTime? FilledAt { get; private set; }

    private Order() { }

    public Order(
        Guid portfolioId,
        string symbol,
        string side,
        string type,
        decimal quantity,
        decimal price,
        decimal commission,
        decimal total,
        string status)
    {
        Id = Guid.NewGuid();
        PortfolioId = portfolioId;
        Symbol = symbol;
        Side = side;
        Type = type;
        Quantity = quantity;
        Price = price;
        Commission = commission;
        Total = total;
        Status = status;
        CreatedAt = DateTime.UtcNow;
        
        if (status == "Filled")
            FilledAt = DateTime.UtcNow;
    }

    public void MarkAsFilled()
    {
        Status = "Filled";
        FilledAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        Status = "Cancelled";
    }
}
