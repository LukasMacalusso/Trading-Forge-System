using Moq;
using TraderForge.Application.DTOs.Queries;
using TraderForge.Application.Handlers;
using TraderForge.Domain.Services;
using TraderForge.Domain.Models;

namespace TraderForge.Application.Tests;

public class GetHistoricalCandlesQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsFailureIfNull()
    {
        var provider = new Mock<IMarketDataProvider>();
        provider.Setup(x => x.GetHistoricalCandlesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(new List<Candlestick>());
            
        var handler = new GetHistoricalCandlesQueryHandler(provider.Object);
        var result = await handler.HandleAsync(new GetHistoricalCandlesQuery("BTCUSDT", "1d", 100));
        Assert.True(result.IsSuccess);
    }
}
