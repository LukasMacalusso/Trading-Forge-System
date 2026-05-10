namespace TraderForge.Application.DTOs;

public class AddPortfolioAssetCommand
{
    public string TraderId { get; set; }
    public string Symbol { get; set; }
    public decimal Quantity { get; set; }
    public decimal EntryPrice { get; set; }
}
