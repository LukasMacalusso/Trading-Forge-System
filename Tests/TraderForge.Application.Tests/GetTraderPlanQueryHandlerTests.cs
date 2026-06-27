using Moq;
using TraderForge.Domain.Common;
using TraderForge.Application.DTOs;
using TraderForge.Application.Handlers;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Repositories;

namespace TraderForge.Application.Tests;

public class GetTraderPlanQueryHandlerTests
{
    private readonly Mock<ITraderRepository> _traderRepoMock;
    private readonly GetTraderPlanQueryHandler _handler;

    public GetTraderPlanQueryHandlerTests()
    {
        _traderRepoMock = new Mock<ITraderRepository>();
        _handler = new GetTraderPlanQueryHandler(_traderRepoMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenTraderExists_ReturnsPlan()
    {
        var traderId = Guid.NewGuid().ToString();
        var planId = Guid.NewGuid();
        var plan = new SubscriptionPlan(planId, "Basic", 9.99m, 10000m, 2, 5, false);
        var trader = new Trader(traderId, "test@test.com");
        trader.AssignSubscriptionPlan(plan);

        var prop = typeof(Trader).GetProperty("SubscriptionPlan", 
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        prop!.SetValue(trader, plan);

        _traderRepoMock.Setup(x => x.GetByIdIncludeSubPlanAsync(traderId)).ReturnsAsync(trader);

        var query = new GetTraderPlanQuery { TraderId = traderId };
        var result = await _handler.HandleAsync(query);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Basic", result.Value!.Name);
    }

    [Fact]
    public async Task HandleAsync_WhenTraderNotFound_ReturnsFailure()
    {
        _traderRepoMock.Setup(x => x.GetByIdIncludeSubPlanAsync(It.IsAny<string>()))
            .ReturnsAsync((Trader?)null!);

        var query = new GetTraderPlanQuery { TraderId = "nonexistent" };
        var result = await _handler.HandleAsync(query);

        Assert.False(result.IsSuccess);
        Assert.Equal("Trader not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task HandleAsync_WhenExceptionThrown_ReturnsFailure()
    {
        _traderRepoMock.Setup(x => x.GetByIdIncludeSubPlanAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error"));

        var query = new GetTraderPlanQuery { TraderId = "id" };
        var result = await _handler.HandleAsync(query);

        Assert.False(result.IsSuccess);
        Assert.Equal("Database error", result.ErrorMessage);
    }
}
