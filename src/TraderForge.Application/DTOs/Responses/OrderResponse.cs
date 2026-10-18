using TraderForge.Domain.Entities;

namespace TraderForge.Application.DTOs.Responses;

public class OrderResponse
{
    public Guid Id { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public string Side { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Commission { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? FilledAt { get; set; }

    public static OrderResponse FromEntity(Order order) => new()
    {
        Id = order.Id,
        Symbol = order.Symbol,
        Side = order.Side.ToString(),
        Type = order.Type.ToString(),
        Quantity = order.Quantity,
        Price = order.Price,
        Commission = order.Commission,
        Total = order.Total,
        Status = order.Status.ToString(),
        CreatedAt = order.CreatedAt,
        FilledAt = order.FilledAt
    };
}
