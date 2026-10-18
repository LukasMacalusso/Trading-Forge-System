namespace TraderForge.Application.DTOs;

public class BuyPositionCommand
{
    public required string TraderId { get; set; }
    public required string Symbol { get; set; }
    public required decimal Quantity { get; set; }
}
