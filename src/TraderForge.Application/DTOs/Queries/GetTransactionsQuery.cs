namespace TraderForge.Application.DTOs;

public class GetTransactionsQuery
{
    public string TraderId { get; set; } = string.Empty;
    public Guid? PortfolioId { get; set; }
}
