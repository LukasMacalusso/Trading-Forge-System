using Moq;
using TraderForge.Domain.Common;
using TraderForge.Application.DTOs.Queries;
using TraderForge.Application.DTOs.Responses;
using TraderForge.Application.Handlers;
using TraderForge.Domain.Constants;
using TraderForge.Domain.Services;
namespace TraderForge.Application.Tests;

public class GetMarketPricesQueryHandlerTests
{
    private readonly Mock<IMarketService> _marketServiceMock;
    private readonly GetMarketPricesQueryHandler _handler;

    public GetMarketPricesQueryHandlerTests()
    {
        _marketServiceMock = new Mock<IMarketService>();
        _handler = new GetMarketPricesQueryHandler(_marketServiceMock.Object);
    }

    [Fact]
    public async Task GetMarketPricesAsync_WhenSymbolsRequested_ReturnsOnlyRequestedPrices()
    {
        var cacheItem = new MarketPriceCacheItem
        {
            Prices = new Dictionary<string, decimal>
            {
                { "BTCUSDT", 6500 },
                { "ETHUSDT", 3450 },
                { "SOLUSDT", 145 },
                { "BNBUSDT", 580 },
                { "XRPUSDT", 0 },
            },
            LastUpdated = DateTime.UtcNow
        };
        
        _marketServiceMock
            .Setup(x => x.GetPricesAsync())
            .ReturnsAsync(cacheItem);

        var query = new GetMarketPricesQuery
        {
            Symbols = new List<string> { "BTCUSDT", "ETHUSDT" }
        };
        
        ResultGeneric<MarketPricesResponse> result = await _handler.HandleAsync(query);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Contains("BTCUSDT", result.Value.Prices.Keys);
        Assert.Contains("ETHUSDT", result.Value.Prices.Keys);
        Assert.Equal(6500, result.Value.Prices["BTCUSDT"]);
        Assert.Equal(3450, result.Value.Prices["ETHUSDT"]);
    }
}
