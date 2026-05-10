namespace TraderForge.Application.DTOs;

public class AddPositionCommand
{
    public required string TraderId { get; set; }
    public required string Symbol { get; set; }
    public required decimal Quantity { get; set; }
    public required decimal EntryPrice { get; set; }
}
