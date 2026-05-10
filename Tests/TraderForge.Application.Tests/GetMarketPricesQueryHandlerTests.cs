using Moq;
using TraderForge.Application.Common;
using TraderForge.Application.DTOs.Queries;
using TraderForge.Application.Handlers;
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
        var allPrices = new Dictionary<string, decimal>
        {
            { "BTCUSDT", 6500 },
            { "ETHUSDT", 3450 },
            { "SOLUSDT", 145 },
            { "BNBUSDT", 580 },
            { "XRPUSDT", 0 },
        };
        
        _marketServiceMock
            .Setup(x => x.GetPricesAsync())
            .ReturnsAsync(allPrices);

        var query = new GetMarketPricesQuery
        {
            Symbols = new List<string> { "BTCUSDT", "ETHUSDT" }
        };

        ResultGeneric<Dictionary<string, decimal>> result = await _handler.HandleAsync(query);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Contains("BTCUSDT", result.Value.Keys);
        Assert.Contains("ETHUSDT", result.Value.Keys);
        Assert.Equal(6500, result.Value["BTCUSDT"]);
        Assert.Equal(3450, result.Value["ETHUSDT"]);
    }
}
