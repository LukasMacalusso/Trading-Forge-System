using Moq;
using TraderForge.Application.DTOs;
using TraderForge.Application.Handlers;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Repositories;
using TraderForge.Domain.Services;
using Xunit;

namespace TraderForge.Application.Tests;

public class ApprovePendingOperationCommandHandlerTests
{
    private readonly Mock<IPendingOperationRepository> _pendingRepoMock;
    private readonly Mock<ICommissionService> _commissionServiceMock;
    private readonly ApprovePendingOperationCommandHandler _handler;

    public ApprovePendingOperationCommandHandlerTests()
    {
        _pendingRepoMock = new Mock<IPendingOperationRepository>();
        _commissionServiceMock = new Mock<ICommissionService>();
        _commissionServiceMock.Setup(x => x.Calculate(It.IsAny<decimal>())).Returns(0m);
        _handler = new ApprovePendingOperationCommandHandler(_pendingRepoMock.Object, _commissionServiceMock.Object);
    }

    [Fact]
    public async Task HandleAsync_OperationNotFound_ReturnsFailure()
    {
        _pendingRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((PendingOperation?)null);

        var result = await _handler.HandleAsync(new ApprovePendingOperationCommand { TraderId = "t1", OperationId = Guid.NewGuid() });

        Assert.False(result.IsSuccess);
        Assert.Contains("not found", result.ErrorMessage);
    }

    [Fact]
    public async Task HandleAsync_AlreadyResolved_ReturnsFailure()
    {
        var op = CreatePendingOperation("t1", "buy", false);
        op.Resolve();
        _pendingRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(op);

        var result = await _handler.HandleAsync(new ApprovePendingOperationCommand { TraderId = "t1", OperationId = op.Id });

        Assert.False(result.IsSuccess);
        Assert.Contains("not found", result.ErrorMessage);
    }

    [Fact]
    public async Task HandleAsync_WrongTrader_ReturnsForbidden()
    {
        var op = CreatePendingOperation("trader-other", "buy", false);
        _pendingRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(op);

        var result = await _handler.HandleAsync(new ApprovePendingOperationCommand { TraderId = "t1", OperationId = op.Id });

        Assert.False(result.IsSuccess);
        Assert.Equal("Forbidden.", result.ErrorMessage);
    }

    [Fact]
    public async Task HandleAsync_Expired_ReturnsFailure()
    {
        var op = CreatePendingOperation("t1", "buy", true);
        _pendingRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(op);

        var result = await _handler.HandleAsync(new ApprovePendingOperationCommand { TraderId = "t1", OperationId = op.Id });

        Assert.False(result.IsSuccess);
        Assert.Contains("expired", result.ErrorMessage);
        Assert.True(op.IsResolved);
    }

    [Fact]
    public async Task HandleAsync_BuyAction_ExecutesBuyPosition()
    {
        var op = CreatePendingOperation("t1", "buy", false);
        _pendingRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(op);

        var result = await _handler.HandleAsync(new ApprovePendingOperationCommand { TraderId = "t1", OperationId = op.Id });

        Assert.True(result.IsSuccess);
        Assert.True(op.IsResolved);
    }

    [Fact]
    public async Task HandleAsync_SellAction_ExecutesSellPosition()
    {
        var portfolio = new Portfolio("t1", 100000m);
        portfolio.BuyPosition("BTC", 2m, 50000m, _commissionServiceMock.Object);
        var op = new PendingOperation(Guid.NewGuid(), portfolio.Id, Guid.NewGuid(), "Strat", "BTC", "sell", 1m, 60000m, DateTime.UtcNow.AddMinutes(5));
        typeof(PendingOperation).GetProperty("Portfolio")!.SetValue(op, portfolio);
        _pendingRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(op);

        var result = await _handler.HandleAsync(new ApprovePendingOperationCommand { TraderId = "t1", OperationId = op.Id });

        Assert.True(result.IsSuccess);
        Assert.True(op.IsResolved);
    }

    private static PendingOperation CreatePendingOperation(string traderId, string action, bool expired)
    {
        var portfolio = new Portfolio(traderId, 100000m);
        var expiresAt = expired ? DateTime.UtcNow.AddMinutes(-1) : DateTime.UtcNow.AddMinutes(5);
        var op = new PendingOperation(Guid.NewGuid(), portfolio.Id, Guid.NewGuid(), "Strat", "BTC", action, 1m, 50000m, expiresAt);
        typeof(PendingOperation).GetProperty("Portfolio")!.SetValue(op, portfolio);
        return op;
    }
}
