namespace TraderForge.Domain.Constants;

public class MarketPriceCacheItem
{
    public Dictionary<string, decimal> Prices { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}