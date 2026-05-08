namespace TraderForge.Domain.Entities; 

public class MarketAsset
{
    public int Id { get; set; }
    public string Symbol { get; set; }
    public string Name { get; set; }
    public decimal CurrentPrice { get; set; }
    public DateTime LastUpdated { get; set; }
}