namespace TraderForge.Domain.Entities;

public class Position
{
    public Guid Id { get; private set; }
    public string Symbol { get; private set; } = null!;
    public decimal Quantity { get; private set; }
    public decimal EntryPrice { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public Guid PortfolioId { get; private set; }
    public Portfolio Portfolio { get; private set; } = null!;

    private Position() { }


    public Position(Guid id, string symbol, decimal quantity, decimal entryPrice, Guid portfolioId)
    {
        Id = id;
        Symbol = symbol;
        Quantity = quantity;
        EntryPrice = entryPrice;
        PortfolioId = portfolioId;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(decimal additionalQuantity, decimal newEntryPrice)
    {
        var totalCost = (Quantity * EntryPrice) + (additionalQuantity * newEntryPrice);
        Quantity += additionalQuantity;
        EntryPrice = totalCost / Quantity;
    }

    public void ReduceQuantity(decimal sellQuantity)
    {
        if (sellQuantity > Quantity)
            throw new InvalidOperationException("Cannot reduce below zero.");

        Quantity -= sellQuantity;
    }
}
