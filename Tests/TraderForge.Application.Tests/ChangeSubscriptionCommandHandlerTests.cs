using Moq;
using TraderForge.Domain.Common;
using TraderForge.Application.DTOs;
using TraderForge.Application.Handlers;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Repositories;
using TraderForge.Domain.Services;

namespace TraderForge.Application.Tests;

public class ChangeSubscriptionCommandHandlerTests
{
    private readonly Mock<ITraderRepository> _traderRepoMock;
    private readonly Mock<ISubscriptionPlanRepository> _planRepoMock;
    private readonly Mock<ISubscriptionLimitGuard> _limitGuardMock;
    private readonly ChangeSubscriptionCommandHandler _handler;

    public ChangeSubscriptionCommandHandlerTests()
    {
        _traderRepoMock = new Mock<ITraderRepository>();
        _planRepoMock = new Mock<ISubscriptionPlanRepository>();
        _limitGuardMock = new Mock<ISubscriptionLimitGuard>();
        _handler = new ChangeSubscriptionCommandHandler(
            _traderRepoMock.Object, _planRepoMock.Object, _limitGuardMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenValid_ShouldChangePlan()
    {
        var traderId = Guid.NewGuid().ToString();
        var planId = Guid.NewGuid();
        var trader = new Trader(traderId, "test@test.com");
        var newPlan = new SubscriptionPlan(planId, "Pro", 29.99m, 50000m, 10, 20, false);

        _traderRepoMock.Setup(x => x.GetByIdIncludeAllAsync(traderId)).ReturnsAsync(trader);
        _planRepoMock.Setup(x => x.GetByIdAsync(planId)).ReturnsAsync(newPlan);
        _limitGuardMock.Setup(x => x.CanSwitchToPlanAsync(traderId, newPlan)).ReturnsAsync(true);

        var command = new ChangeSubscriptionCommand { TraderId = traderId, NewPlanId = planId };
        var result = await _handler.HandleAsync(command);

        Assert.True(result.IsSuccess);
        _traderRepoMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenTraderNotFound_ReturnsFailure()
    {
        _traderRepoMock.Setup(x => x.GetByIdIncludeAllAsync(It.IsAny<string>())).ReturnsAsync((Trader?)null!);

        var command = new ChangeSubscriptionCommand { TraderId = "nonexistent", NewPlanId = Guid.NewGuid() };
        var result = await _handler.HandleAsync(command);

        Assert.False(result.IsSuccess);
        Assert.Equal("Trader not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task HandleAsync_WhenPlanNotFound_ReturnsFailure()
    {
        var traderId = Guid.NewGuid().ToString();
        var trader = new Trader(traderId, "test@test.com");

        _traderRepoMock.Setup(x => x.GetByIdIncludeAllAsync(traderId)).ReturnsAsync(trader);
        _planRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((SubscriptionPlan?)null!);

        var command = new ChangeSubscriptionCommand { TraderId = traderId, NewPlanId = Guid.NewGuid() };
        var result = await _handler.HandleAsync(command);

        Assert.False(result.IsSuccess);
        Assert.Equal("Subscription Plan not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task HandleAsync_WhenLimitGuardBlocks_ReturnsFailure()
    {
        var traderId = Guid.NewGuid().ToString();
        var planId = Guid.NewGuid();
        var trader = new Trader(traderId, "test@test.com");
        var newPlan = new SubscriptionPlan(planId, "Pro", 29.99m, 50000m, 10, 20, false);

        _traderRepoMock.Setup(x => x.GetByIdIncludeAllAsync(traderId)).ReturnsAsync(trader);
        _planRepoMock.Setup(x => x.GetByIdAsync(planId)).ReturnsAsync(newPlan);
        _limitGuardMock.Setup(x => x.CanSwitchToPlanAsync(traderId, newPlan)).ReturnsAsync(false);

        var command = new ChangeSubscriptionCommand { TraderId = traderId, NewPlanId = planId };
        var result = await _handler.HandleAsync(command);

        Assert.False(result.IsSuccess);
        Assert.Contains("Cannot switch plan", result.ErrorMessage);
    }

    [Fact]
    public async Task HandleAsync_WhenExceptionThrown_ReturnsFailure()
    {
        _traderRepoMock.Setup(x => x.GetByIdIncludeAllAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error"));

        var command = new ChangeSubscriptionCommand { TraderId = "id", NewPlanId = Guid.NewGuid() };
        var result = await _handler.HandleAsync(command);

        Assert.False(result.IsSuccess);
        Assert.Equal("Database error", result.ErrorMessage);
    }
}
