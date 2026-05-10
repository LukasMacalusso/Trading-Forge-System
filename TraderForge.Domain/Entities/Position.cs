namespace TraderForge.Domain.Entities;

public class Position
{
    public Guid Id { get; private set; }
    public string Symbol { get; private set; }
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
}
