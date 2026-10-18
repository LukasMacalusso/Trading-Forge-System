namespace TraderForge.Application.DTOs;

public class GetStrategiesQuery
{
    public string TraderId { get; set; } = string.Empty;
    public Guid? PortfolioId { get; set; }
}
