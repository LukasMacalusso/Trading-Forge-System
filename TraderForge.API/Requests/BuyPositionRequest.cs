using TraderForge.Application.DTOs;

namespace TraderForge.API.Requests;

public class BuyPositionRequest
{
    public string Symbol { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal EntryPrice { get; set; }

    public BuyPositionCommand ToCommand(string traderId) => new()
    {
        TraderId = traderId,
        Symbol = Symbol,
        Quantity = Quantity,
        EntryPrice = EntryPrice
    };
}
