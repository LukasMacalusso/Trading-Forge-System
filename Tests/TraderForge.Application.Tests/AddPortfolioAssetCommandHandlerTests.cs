using Moq;
using TraderForge.Application.Common;
using TraderForge.Application.DTOs;
using TraderForge.Application.Handlers;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Repositories;
using TraderForge.Domain.Services;

namespace TraderForge.Application.Tests;

public class AddPortfolioAssetCommandHandlerTests
{
    private readonly Mock<IPortfolioAssetRepository> _assetRepoMock;
    private readonly Mock<ITraderRepository> _traderRepoMock;
    private readonly Mock<ISubscriptionLimitGuard> _limitGuardMock;
    private readonly AddPortfolioAssetCommandHandler _handler;

    public AddPortfolioAssetCommandHandlerTests()
    {
        _assetRepoMock = new Mock<IPortfolioAssetRepository>();
        _traderRepoMock = new Mock<ITraderRepository>();
        _limitGuardMock = new Mock<ISubscriptionLimitGuard>();
        _handler = new AddPortfolioAssetCommandHandler(
            _assetRepoMock.Object, _traderRepoMock.Object, _limitGuardMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenValid_ShouldAddAsset()
    {
        var traderId = Guid.NewGuid().ToString();
        var trader = new Trader(traderId, "test@test.com");
        trader.Portfolios.Add(new Portfolio(traderId, 10000m));

        _limitGuardMock.Setup(x => x.CanAddAssetAsync(traderId)).ReturnsAsync(true);
        _traderRepoMock.Setup(x => x.GetByIdIncludePortfolioAsync(traderId)).ReturnsAsync(trader);

        var command = new AddPortfolioAssetCommand
        { TraderId = traderId, Symbol = "BTCUSDT", Quantity = 1m, EntryPrice = 50000m };
        var result = await _handler.HandleAsync(command);

        Assert.True(result.IsSuccess);
        _assetRepoMock.Verify(x => x.AddAsync(It.Is<PortfolioAsset>(
            a => a.Symbol == "BTCUSDT" && a.Quantity == 1m && a.EntryPrice == 50000m)), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenLimitReached_ReturnsFailure()
    {
        _limitGuardMock.Setup(x => x.CanAddAssetAsync(It.IsAny<string>())).ReturnsAsync(false);

        var command = new AddPortfolioAssetCommand
        { TraderId = "id", Symbol = "BTCUSDT", Quantity = 1m, EntryPrice = 50000m };
        var result = await _handler.HandleAsync(command);

        Assert.False(result.IsSuccess);
        Assert.Contains("maximum active assets exceeded", result.ErrorMessage);
    }

    [Fact]
    public async Task HandleAsync_WhenNoActivePortfolio_ReturnsFailure()
    {
        var traderId = Guid.NewGuid().ToString();
        var trader = new Trader(traderId, "test@test.com");

        _limitGuardMock.Setup(x => x.CanAddAssetAsync(traderId)).ReturnsAsync(true);
        _traderRepoMock.Setup(x => x.GetByIdIncludePortfolioAsync(traderId)).ReturnsAsync(trader);

        var command = new AddPortfolioAssetCommand
        { TraderId = traderId, Symbol = "BTCUSDT", Quantity = 1m, EntryPrice = 50000m };
        var result = await _handler.HandleAsync(command);

        Assert.False(result.IsSuccess);
        Assert.Equal("No active portfolio found.", result.ErrorMessage);
    }

    [Fact]
    public async Task HandleAsync_WhenExceptionThrown_ReturnsFailure()
    {
        _limitGuardMock.Setup(x => x.CanAddAssetAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Unexpected error"));

        var command = new AddPortfolioAssetCommand
        { TraderId = "id", Symbol = "BTCUSDT", Quantity = 1m, EntryPrice = 50000m };
        var result = await _handler.HandleAsync(command);

        Assert.False(result.IsSuccess);
        Assert.Equal("Unexpected error", result.ErrorMessage);
    }
}
