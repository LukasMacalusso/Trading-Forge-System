using Microsoft.Extensions.Caching.Memory;
using TraderForge.Domain.Constants;
using TraderForge.Domain.Services;

namespace TraderForge.Infrastructure.Services;

public class CachedMarketService : IMarketService
{
    private readonly IMemoryCache _cache;

    public CachedMarketService(IMemoryCache cache) => _cache = cache;

    public async Task<MarketPriceCacheItem> GetPricesAsync()
    {
        _cache.TryGetValue(CacheKeys.MarketPrices, out MarketPriceCacheItem? item);
        return item ?? new MarketPriceCacheItem();
    }

    public bool IsMarketOpen(string symbol)
    {
        // Crypto markets are 24/7.
        return true;
    }

}
