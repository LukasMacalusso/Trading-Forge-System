using Moq;
using TraderForge.Domain.Common;
using TraderForge.Application.DTOs;
using TraderForge.Application.Handlers;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Repositories;
using TraderForge.Domain.Services;

namespace TraderForge.Application.Tests;

public class CreateStrategyCommandHandlerTests
{
    private readonly Mock<IStrategyRepository> _strategyRepoMock;
    private readonly Mock<ITraderRepository> _traderRepoMock;
    private readonly Mock<ISubscriptionLimitGuard> _limitGuardMock;
    private readonly CreateStrategyCommandHandler _handler;

    public CreateStrategyCommandHandlerTests()
    {
        _strategyRepoMock = new Mock<IStrategyRepository>();
        _traderRepoMock = new Mock<ITraderRepository>();
        _limitGuardMock = new Mock<ISubscriptionLimitGuard>();
        _handler = new CreateStrategyCommandHandler(
            _strategyRepoMock.Object, _traderRepoMock.Object, _limitGuardMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenValid_ShouldCreateStrategy()
    {
        var traderId = Guid.NewGuid().ToString();
        var trader = new Trader(traderId, "test@test.com");
        trader.Portfolios.Add(new Portfolio(traderId, 10000m));

        _limitGuardMock.Setup(x => x.CanAddStrategyAsync(traderId)).ReturnsAsync(true);
        _traderRepoMock.Setup(x => x.GetByIdIncludePortfolioAsync(traderId)).ReturnsAsync(trader);

        var command = new CreateStrategyCommand { TraderId = traderId, Name = "My Strategy" };
        var result = await _handler.HandleAsync(command);

        Assert.True(result.IsSuccess);
        _strategyRepoMock.Verify(x => x.AddAsync(It.Is<Strategy>(s => s.Name == "My Strategy")), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenLimitReached_ReturnsFailure()
    {
        _limitGuardMock.Setup(x => x.CanAddStrategyAsync(It.IsAny<string>())).ReturnsAsync(false);

        var command = new CreateStrategyCommand { TraderId = "id", Name = "Strategy" };
        var result = await _handler.HandleAsync(command);

        Assert.False(result.IsSuccess);
        Assert.Contains("maximum active strategies exceeded", result.ErrorMessage);
    }

    [Fact]
    public async Task HandleAsync_WhenNoActivePortfolio_ReturnsFailure()
    {
        var traderId = Guid.NewGuid().ToString();
        var trader = new Trader(traderId, "test@test.com");

        _limitGuardMock.Setup(x => x.CanAddStrategyAsync(traderId)).ReturnsAsync(true);
        _traderRepoMock.Setup(x => x.GetByIdIncludePortfolioAsync(traderId)).ReturnsAsync(trader);

        var command = new CreateStrategyCommand { TraderId = traderId, Name = "Strategy" };
        var result = await _handler.HandleAsync(command);

        Assert.False(result.IsSuccess);
        Assert.Equal("No active portfolio found.", result.ErrorMessage);
    }

    [Fact]
    public async Task HandleAsync_WhenExceptionThrown_ReturnsFailure()
    {
        _limitGuardMock.Setup(x => x.CanAddStrategyAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Unexpected error"));

        var command = new CreateStrategyCommand { TraderId = "id", Name = "Strategy" };
        var result = await _handler.HandleAsync(command);

        Assert.False(result.IsSuccess);
        Assert.Equal("Unexpected error", result.ErrorMessage);
    }
}
