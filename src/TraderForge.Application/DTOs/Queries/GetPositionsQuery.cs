namespace TraderForge.Application.DTOs;

public class GetPositionsQuery
{
    public required string TraderId { get; set; }
    public Guid? PortfolioId { get; set; }
}
