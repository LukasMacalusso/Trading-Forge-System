using Moq;
using TraderForge.Application.Common;
using TraderForge.Application.DTOs;
using TraderForge.Application.Handlers;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Repositories;

namespace TraderForge.Application.Tests;

public class GetStrategiesQueryHandlerTests
{
    private readonly Mock<IStrategyRepository> _strategyRepoMock;
    private readonly GetStrategiesQueryHandler _handler;

    public GetStrategiesQueryHandlerTests()
    {
        _strategyRepoMock = new Mock<IStrategyRepository>();
        _handler = new GetStrategiesQueryHandler(_strategyRepoMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnStrategies()
    {
        var traderId = Guid.NewGuid().ToString();
        var portfolioId = Guid.NewGuid();
        var strategies = new List<Strategy>
        {
            new(Guid.NewGuid(), "Strategy 1", portfolioId),
            new(Guid.NewGuid(), "Strategy 2", portfolioId),
        };

        _strategyRepoMock.Setup(x => x.GetByTraderIdAsync(traderId)).ReturnsAsync(strategies);

        var query = new GetStrategiesQuery { TraderId = traderId };
        var result = await _handler.HandleAsync(query);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
    }

    [Fact]
    public async Task HandleAsync_WhenExceptionThrown_ReturnsFailure()
    {
        _strategyRepoMock.Setup(x => x.GetByTraderIdAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error"));

        var query = new GetStrategiesQuery { TraderId = "id" };
        var result = await _handler.HandleAsync(query);

        Assert.False(result.IsSuccess);
        Assert.Equal("Database error", result.ErrorMessage);
    }
}
