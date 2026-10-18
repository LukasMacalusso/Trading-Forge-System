namespace TraderForge.Application.DTOs;

public class GetOrdersQuery
{
    public string TraderId { get; set; } = string.Empty;
    public Guid? PortfolioId { get; set; }
}
