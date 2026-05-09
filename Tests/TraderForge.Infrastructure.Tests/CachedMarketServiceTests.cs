using Moq;
using Microsoft.Extensions.Caching.Memory;
using TraderForge.Domain.Constants;
using TraderForge.Infrastructure.Services;

namespace TraderForge.Infrastructure.Tests;

public class CachedMarketServiceTests
{
    private readonly IMemoryCache _cache;
    private readonly CachedMarketService _service;
    
    public CachedMarketServiceTests()
    {
        _cache = new MemoryCache(new MemoryCacheOptions());
        _service = new CachedMarketService(_cache);
    }
    
    [Fact]
    public async Task GetPricesAsync_WhenCacheHasPrices_ReturnsCachedPrices()
    {
        var expectedPrices = new Dictionary<string, decimal>
        {
            { "BTCUSDT", 65432.10m },
            { "ETHUSDT", 3456.78m },
        };

        _cache.Set(CacheKeys.MarketPrices, expectedPrices);
        var result = await _service.GetPricesAsync();
        Assert.Equal(expectedPrices, result);
    }
}
