using Moq;
using TraderForge.Application.Common;
using TraderForge.Application.DTOs;
using TraderForge.Application.Handlers;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Repositories;

namespace TraderForge.Application.Tests;

public class UpdateStrategyStateCommandHandlerTests
{
    private readonly Mock<IStrategyRepository> _strategyRepoMock;
    private readonly UpdateStrategyStateCommandHandler _handler;

    public UpdateStrategyStateCommandHandlerTests()
    {
        _strategyRepoMock = new Mock<IStrategyRepository>();
        _handler = new UpdateStrategyStateCommandHandler(_strategyRepoMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenActivating_ShouldActivateStrategy()
    {
        var strategyId = Guid.NewGuid();
        var strategy = new Strategy(strategyId, "Test", Guid.NewGuid());
        strategy.Deactivate();

        _strategyRepoMock.Setup(x => x.GetByIdAsync(strategyId)).ReturnsAsync(strategy);

        var command = new UpdateStrategyStateCommand { StrategyId = strategyId, IsActive = true };
        var result = await _handler.HandleAsync(command);

        Assert.True(result.IsSuccess);
        Assert.True(strategy.IsActive);
        _strategyRepoMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenDeactivating_ShouldDeactivateStrategy()
    {
        var strategyId = Guid.NewGuid();
        var strategy = new Strategy(strategyId, "Test", Guid.NewGuid());

        _strategyRepoMock.Setup(x => x.GetByIdAsync(strategyId)).ReturnsAsync(strategy);

        var command = new UpdateStrategyStateCommand { StrategyId = strategyId, IsActive = false };
        var result = await _handler.HandleAsync(command);

        Assert.True(result.IsSuccess);
        Assert.False(strategy.IsActive);
        _strategyRepoMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenStrategyNotFound_ReturnsFailure()
    {
        _strategyRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Strategy?)null!);

        var command = new UpdateStrategyStateCommand { StrategyId = Guid.NewGuid(), IsActive = true };
        var result = await _handler.HandleAsync(command);

        Assert.False(result.IsSuccess);
        Assert.Equal("Strategy not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task HandleAsync_WhenExceptionThrown_ReturnsFailure()
    {
        _strategyRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ThrowsAsync(new Exception("Database error"));

        var command = new UpdateStrategyStateCommand { StrategyId = Guid.NewGuid(), IsActive = true };
        var result = await _handler.HandleAsync(command);

        Assert.False(result.IsSuccess);
        Assert.Equal("Database error", result.ErrorMessage);
    }
}
