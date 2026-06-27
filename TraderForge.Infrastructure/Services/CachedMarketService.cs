using Microsoft.Extensions.Caching.Memory;
using TraderForge.Domain.Constants;
using TraderForge.Domain.Services;

namespace TraderForge.Infrastructure.Services;

public class CachedMarketService : IMarketService
{
    private readonly IMemoryCache _cache;
    
    public CachedMarketService(IMemoryCache cache) => _cache = cache;
    
    public async Task<Dictionary<string, decimal>> GetPricesAsync()
    {
        _cache.TryGetValue(CacheKeys.MarketPrices, out Dictionary<string, decimal>? prices);
        return prices ?? new Dictionary<string, decimal>();
    }
    
    public bool IsMarketOpen(string symbol)
    {
        // Crypto markets are 24/7.
        return true;
    }

}