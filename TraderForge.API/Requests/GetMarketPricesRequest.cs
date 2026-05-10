namespace TraderForge.API.Requests;

public class GetMarketPricesRequest
{
    public List<string> Symbols { get; set; } = new();
}
