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

        var expectedCacheItem = new MarketPriceCacheItem
        {
            Prices = expectedPrices,
            LastUpdated = DateTime.UtcNow
        };

        _cache.Set(CacheKeys.MarketPrices, expectedCacheItem);

        var result = await _service.GetPricesAsync();

        Assert.NotNull(result);
        Assert.Equal(expectedPrices, result.Prices);
    }
    [Fact]
    public async Task GetPricesAsync_WhenCacheIsEmpty_ReturnsEmptyDictionary()
    {
        var result = await _service.GetPricesAsync();
        Assert.NotNull(result.Prices);
        Assert.Empty(result.Prices);
    }
}
