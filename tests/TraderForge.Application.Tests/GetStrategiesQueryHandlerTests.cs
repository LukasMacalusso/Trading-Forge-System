using Moq;
using TraderForge.Application.DTOs;
using TraderForge.Application.Handlers;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace TraderForge.Application.Tests;

public class GetStrategiesQueryHandlerTests
{
    private readonly Mock<ITraderRepository> _traderRepoMock;
    private readonly Mock<IStrategyRepository> _strategyRepoMock;
    private readonly GetStrategiesQueryHandler _handler;

    public GetStrategiesQueryHandlerTests()
    {
        _traderRepoMock = new Mock<ITraderRepository>();
        _strategyRepoMock = new Mock<IStrategyRepository>();
        _handler = new GetStrategiesQueryHandler(_traderRepoMock.Object, _strategyRepoMock.Object);
    }

    private static Trader CreateTraderWithActivePortfolio(string traderId, out Guid portfolioId)
    {
        var portfolio = new Portfolio(traderId, 10000m);
        portfolioId = portfolio.Id;
        var trader = new Trader(traderId, "test@test.com");
        trader.Portfolios.Add(portfolio);
        return trader;
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnStrategies()
    {
        var traderId = Guid.NewGuid().ToString();
        var trader = CreateTraderWithActivePortfolio(traderId, out var portfolioId);
        var strategies = new List<Strategy>
        {
            new(Guid.NewGuid(), "Strategy 1", portfolioId),
            new(Guid.NewGuid(), "Strategy 2", portfolioId),
        };

        _traderRepoMock.Setup(x => x.GetByIdIncludePortfolioAsync(traderId)).ReturnsAsync(trader);
        _strategyRepoMock.Setup(x => x.GetByPortfolioIdAsync(portfolioId)).ReturnsAsync(strategies);

        var query = new GetStrategiesQuery { TraderId = traderId };
        var result = await _handler.HandleAsync(query);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
    }

    [Fact]
    public async Task HandleAsync_TraderNotFound_ReturnsFailure()
    {
        _traderRepoMock.Setup(x => x.GetByIdIncludePortfolioAsync(It.IsAny<string>())).ReturnsAsync((Trader?)null);

        var query = new GetStrategiesQuery { TraderId = "nonexistent" };
        var result = await _handler.HandleAsync(query);

        Assert.False(result.IsSuccess);
        Assert.Equal("Trader not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task HandleAsync_WhenExceptionThrown_ReturnsFailure()
    {
        _traderRepoMock.Setup(x => x.GetByIdIncludePortfolioAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error"));

        var query = new GetStrategiesQuery { TraderId = "some-id" };
        var result = await _handler.HandleAsync(query);

        Assert.False(result.IsSuccess);
    }
}
