namespace TraderForge.API.Requests;

public class CreateStrategyRequest
{
    public string Name { get; set; }
}

public class UpdateStrategyStateRequest
{
    public bool IsActive { get; set; }
}

public class AddPortfolioAssetRequest
{
    public string Symbol { get; set; }
    public decimal Quantity { get; set; }
    public decimal EntryPrice { get; set; }
}
