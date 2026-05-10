using Moq;
using TraderForge.Application.Common;
using TraderForge.Application.DTOs;
using TraderForge.Application.Handlers;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Repositories;

namespace TraderForge.Application.Tests;

public class GetPortfolioAssetsQueryHandlerTests
{
    private readonly Mock<IPortfolioAssetRepository> _assetRepoMock;
    private readonly GetPortfolioAssetsQueryHandler _handler;

    public GetPortfolioAssetsQueryHandlerTests()
    {
        _assetRepoMock = new Mock<IPortfolioAssetRepository>();
        _handler = new GetPortfolioAssetsQueryHandler(_assetRepoMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnAssets()
    {
        var traderId = Guid.NewGuid().ToString();
        var portfolioId = Guid.NewGuid();
        var assets = new List<PortfolioAsset>
        {
            new(Guid.NewGuid(), "BTCUSDT", 1m, 50000m, portfolioId),
            new(Guid.NewGuid(), "ETHUSDT", 10m, 3000m, portfolioId),
        };

        _assetRepoMock.Setup(x => x.GetByTraderIdAsync(traderId)).ReturnsAsync(assets);

        var query = new GetPortfolioAssetsQuery { TraderId = traderId };
        var result = await _handler.HandleAsync(query);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
    }

    [Fact]
    public async Task HandleAsync_WhenExceptionThrown_ReturnsFailure()
    {
        _assetRepoMock.Setup(x => x.GetByTraderIdAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error"));

        var query = new GetPortfolioAssetsQuery { TraderId = "id" };
        var result = await _handler.HandleAsync(query);

        Assert.False(result.IsSuccess);
        Assert.Equal("Database error", result.ErrorMessage);
    }
}
