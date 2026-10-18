using Moq;
using TraderForge.Domain.Common;
using TraderForge.Application.DTOs;
using TraderForge.Application.Handlers;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Repositories;

namespace TraderForge.Application.Tests;

public class GetActivePortfolioQueryHandlerTests
{
    private readonly Mock<ITraderRepository> _traderRepoMock;
    private readonly GetActivePortfolioQueryHandler _handler;

    public GetActivePortfolioQueryHandlerTests()
    {
        _traderRepoMock = new Mock<ITraderRepository>();
        _handler = new GetActivePortfolioQueryHandler(_traderRepoMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenTraderExists_ReturnsActivePortfolio()
    {
        var traderId = Guid.NewGuid().ToString();
        var trader = new Trader(traderId, "test@test.com");
        var portfolio = new Portfolio(traderId, 10000m);
        trader.Portfolios.Add(portfolio);

        _traderRepoMock.Setup(x => x.GetByIdIncludePortfolioAsync(traderId)).ReturnsAsync(trader);

        var query = new GetActivePortfolioQuery { TraderId = traderId };
        var result = await _handler.HandleAsync(query);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(10000m, result.Value!.VirtualBalance);
    }

    [Fact]
    public async Task HandleAsync_WhenTraderNotFound_ReturnsFailure()
    {
        _traderRepoMock.Setup(x => x.GetByIdIncludePortfolioAsync(It.IsAny<string>()))
            .ReturnsAsync((Trader?)null!);

        var query = new GetActivePortfolioQuery { TraderId = "nonexistent" };
        var result = await _handler.HandleAsync(query);

        Assert.False(result.IsSuccess);
        Assert.Equal("Trader not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task HandleAsync_WhenNoActivePortfolio_ReturnsFailure()
    {
        var traderId = Guid.NewGuid().ToString();
        var trader = new Trader(traderId, "test@test.com");
        var portfolio = new Portfolio(traderId, 10000m);
        portfolio.FreezeSimulation();
        trader.Portfolios.Add(portfolio);

        _traderRepoMock.Setup(x => x.GetByIdIncludePortfolioAsync(traderId)).ReturnsAsync(trader);

        var query = new GetActivePortfolioQuery { TraderId = traderId };
        var result = await _handler.HandleAsync(query);

        Assert.False(result.IsSuccess);
        Assert.Equal("No active portfolio found.", result.ErrorMessage);
    }

    [Fact]
    public async Task HandleAsync_WhenExceptionThrown_ReturnsFailure()
    {
        _traderRepoMock.Setup(x => x.GetByIdIncludePortfolioAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error"));

        var query = new GetActivePortfolioQuery { TraderId = "id" };
        var result = await _handler.HandleAsync(query);

        Assert.False(result.IsSuccess);
        Assert.Equal("Database error", result.ErrorMessage);
    }
}
