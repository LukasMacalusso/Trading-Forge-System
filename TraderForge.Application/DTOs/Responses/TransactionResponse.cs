using TraderForge.Domain.Entities;

namespace TraderForge.Application.DTOs.Responses;

public class TransactionResponse
{
    public Guid Id { get; set; }
    public string Type { get; set; }
    public string? Symbol { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? Price { get; set; }
    public decimal Commission { get; set; }
    public decimal Total { get; set; }
    public decimal BalanceBefore { get; set; }
    public decimal BalanceAfter { get; set; }
    public DateTime CreatedAt { get; set; }

    public static TransactionResponse FromEntity(Transaction t) => new()
    {
        Id = t.Id,
        Type = t.Type,
        Symbol = t.Symbol,
        Quantity = t.Quantity,
        Price = t.Price,
        Commission = t.Commission,
        Total = t.Total,
        BalanceBefore = t.BalanceBefore,
        BalanceAfter = t.BalanceAfter,
        CreatedAt = t.CreatedAt
    };
}
