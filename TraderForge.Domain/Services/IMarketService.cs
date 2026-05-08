namespace TraderForge.Domain.Services;

public interface IMarketService
{
    Task<Dictionary<string, decimal>> GetPricesAsync();
}