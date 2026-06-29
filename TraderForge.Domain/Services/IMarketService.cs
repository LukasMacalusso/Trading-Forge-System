using TraderForge.Domain.Constants;

namespace TraderForge.Domain.Services;

public interface IMarketService
{
    Task<MarketPriceCacheItem> GetPricesAsync();
    bool IsMarketOpen(string symbol);
}
