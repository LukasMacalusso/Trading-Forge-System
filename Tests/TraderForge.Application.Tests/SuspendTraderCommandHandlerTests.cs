using Moq;
using TraderForge.Application.DTOs;
using TraderForge.Application.DTOs.Commands;
using TraderForge.Application.Handlers;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Repositories;

namespace TraderForge.Application.Tests;

public class SuspendTraderCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_TraderNotFound_ReturnsFailure()
    {
        var repoMock = new Mock<ITraderRepository>();
        repoMock.Setup(r => r.GetByIdAsync(It.IsAny<string>())).ReturnsAsync((Trader?)null);

        var handler = new SuspendTraderCommandHandler(repoMock.Object);
        var command = new SuspendTraderCommand("u1", "Violation");

        var result = await handler.HandleAsync(command);

        Assert.False(result.IsSuccess);
        Assert.Equal("Trader not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task HandleAsync_AlreadySuspended_ReturnsFailure()
    {
        var trader = new Trader("u1", "test@test.com");
        trader.Suspend("Old reason");

        var repoMock = new Mock<ITraderRepository>();
        repoMock.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync(trader);

        var handler = new SuspendTraderCommandHandler(repoMock.Object);
        var command = new SuspendTraderCommand("u1", "New violation");

        var result = await handler.HandleAsync(command);

        Assert.False(result.IsSuccess);
        Assert.Equal("Trader is already suspended.", result.ErrorMessage);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task HandleAsync_MissingReason_ReturnsFailure(string? invalidReason)
    {
        var trader = new Trader("u1", "test@test.com");

        var repoMock = new Mock<ITraderRepository>();
        repoMock.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync(trader);

        var handler = new SuspendTraderCommandHandler(repoMock.Object);
        var command = new SuspendTraderCommand("u1", invalidReason);

        var result = await handler.HandleAsync(command);

        Assert.False(result.IsSuccess);
        Assert.Equal("A suspension reason is required.", result.ErrorMessage);
    }

    [Fact]
    public async Task HandleAsync_Success()
    {
        var trader = new Trader("u1", "test@test.com");
        var repoMock = new Mock<ITraderRepository>();
        repoMock.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync(trader);

        var handler = new SuspendTraderCommandHandler(repoMock.Object);
        var command = new SuspendTraderCommand("u1", "Fraudulent activity");

        var result = await handler.HandleAsync(command);

        Assert.True(result.IsSuccess);
        Assert.True(trader.IsSuspended);
        Assert.Equal("Fraudulent activity", trader.SuspensionReason);
        repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}
