namespace TraderForge.API.Requests;

public class BuyPositionRequest
{
    public string Symbol { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
}
