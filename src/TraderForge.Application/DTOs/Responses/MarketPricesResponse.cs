namespace TraderForge.Application.DTOs.Responses;

public class MarketPricesResponse
{
    public Dictionary<string, decimal> Prices { get; set; } = new();
    public bool IsStale { get; set; }
}
