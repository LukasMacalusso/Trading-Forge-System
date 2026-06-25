using TraderForge.Domain.Enums;

namespace TraderForge.Domain.Entities;

public class Order
{
    public Guid Id { get; private set; }
    public Guid PortfolioId { get; private set; }
    public Portfolio Portfolio { get; private set; } = null!;

    public string Symbol { get; private set; }
    public OrderSide Side { get; private set; }
    public OrderType Type { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal Price { get; private set; }
    public decimal Commission { get; private set; }
    public decimal Total { get; private set; }
    public OrderStatus Status { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? FilledAt { get; private set; }

    private Order() { }

    public Order(
        Guid portfolioId,
        string symbol,
        OrderSide side,
        OrderType type,
        decimal quantity,
        decimal price,
        decimal commission,
        decimal total,
        OrderStatus status)
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

        if (status == OrderStatus.Filled)
            FilledAt = DateTime.UtcNow;
    }

    public void MarkAsFilled()
    {
        Status = OrderStatus.Filled;
        FilledAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        Status = OrderStatus.Cancelled;
    }
}
