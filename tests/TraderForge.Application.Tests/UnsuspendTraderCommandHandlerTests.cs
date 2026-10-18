using Moq;
using TraderForge.Application.DTOs.Commands;
using TraderForge.Application.Handlers;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Repositories;

namespace TraderForge.Application.Tests;

public class UnsuspendTraderCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_TraderNotFound_ReturnsFailure()
    {
        var repoMock = new Mock<ITraderRepository>();
        repoMock.Setup(r => r.GetByIdAsync(It.IsAny<string>())).ReturnsAsync((Trader?)null);

        var handler = new UnsuspendTraderCommandHandler(repoMock.Object);
        var command = new UnsuspendTraderCommand("u1");

        var result = await handler.HandleAsync(command);

        Assert.False(result.IsSuccess);
        Assert.Equal("Trader not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task HandleAsync_NotSuspended_ReturnsFailure()
    {
        var trader = new Trader("u1", "test@test.com");
        // By default IsSuspended is false

        var repoMock = new Mock<ITraderRepository>();
        repoMock.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync(trader);

        var handler = new UnsuspendTraderCommandHandler(repoMock.Object);
        var command = new UnsuspendTraderCommand("u1");

        var result = await handler.HandleAsync(command);

        Assert.False(result.IsSuccess);
        Assert.Equal("Trader is not suspended.", result.ErrorMessage);
    }

    [Fact]
    public async Task HandleAsync_Success()
    {
        var trader = new Trader("u1", "test@test.com");
        trader.Suspend("Fraudulent activity");

        var repoMock = new Mock<ITraderRepository>();
        repoMock.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync(trader);

        var handler = new UnsuspendTraderCommandHandler(repoMock.Object);
        var command = new UnsuspendTraderCommand("u1");

        var result = await handler.HandleAsync(command);

        Assert.True(result.IsSuccess);
        Assert.False(trader.IsSuspended);
        Assert.Equal(string.Empty, trader.SuspensionReason);
        repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}
