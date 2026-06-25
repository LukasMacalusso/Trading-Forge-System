namespace TraderForge.Domain.Services;

public interface IMarketDataProvider
{
    Task<Dictionary<string, decimal>> GetPricesAsync();
}