using MediatR;
using Moq;
using TraderForge.Application.DTOs;
using TraderForge.Application.Handlers;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Repositories;

namespace TraderForge.Application.Tests;

public class ResetSimulationCommandHandlerTests
{
    private readonly Mock<ITraderRepository> _traderRepoMock;
    private readonly ResetSimulationCommandHandler _handler;

    public ResetSimulationCommandHandlerTests()
    {
        _traderRepoMock = new Mock<ITraderRepository>();
        _handler = new ResetSimulationCommandHandler(_traderRepoMock.Object, Mock.Of<IPublisher>());
    }

    [Fact]
    public async Task HandleAsync_TraderNotFound_ReturnsFailure()
    {
        _traderRepoMock.Setup(x => x.GetByIdIncludePlanAndPositionsAsync(It.IsAny<string>()))
            .ReturnsAsync((Trader?)null);

        var result = await _handler.HandleAsync(new ResetSimulationCommand { TraderId = "user-1" });

        Assert.False(result.IsSuccess);
        Assert.Equal("Trader not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task HandleAsync_SubscriptionOrPlanNull_ReturnsFailure()
    {
        var trader = new Trader("user-1", "test@test.com");
        _traderRepoMock.Setup(x => x.GetByIdIncludePlanAndPositionsAsync("user-1"))
            .ReturnsAsync(trader);

        var result = await _handler.HandleAsync(new ResetSimulationCommand { TraderId = "user-1" });

        Assert.False(result.IsSuccess);
        Assert.Equal("Active subscription or plan not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task HandleAsync_ValidTrader_ResetsPortfolioAndSaves()
    {
        var trader = new Trader("user-1", "test@test.com");
        var planId = Guid.NewGuid();
        var plan = new SubscriptionPlan(planId, "Pro", 29.99m, 50000m, 10, 20, false);
        var activeSub = new ActiveSubscription("user-1", planId, 30);
        typeof(ActiveSubscription).GetProperty("Plan")!.SetValue(activeSub, plan);
        typeof(Trader).GetProperty("Subscription")!.SetValue(trader, activeSub);

        _traderRepoMock.Setup(x => x.GetByIdIncludePlanAndPositionsAsync("user-1"))
            .ReturnsAsync(trader);

        var result = await _handler.HandleAsync(new ResetSimulationCommand { TraderId = "user-1" });

        Assert.True(result.IsSuccess);
        Assert.Single(trader.Portfolios);
        Assert.Equal(50000m, trader.Portfolios.First().VirtualBalance);
        _traderRepoMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
}
