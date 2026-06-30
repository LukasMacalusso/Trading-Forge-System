using Moq;
using TraderForge.Application.DTOs;
using TraderForge.Application.Handlers;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Repositories;
using Xunit;

namespace TraderForge.Application.Tests;

public class RejectPendingOperationCommandHandlerTests
{
    private readonly Mock<IPendingOperationRepository> _pendingRepoMock;
    private readonly RejectPendingOperationCommandHandler _handler;

    public RejectPendingOperationCommandHandlerTests()
    {
        _pendingRepoMock = new Mock<IPendingOperationRepository>();
        _handler = new RejectPendingOperationCommandHandler(_pendingRepoMock.Object);
    }

    [Fact]
    public async Task HandleAsync_OperationNotFound_ReturnsFailure()
    {
        _pendingRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((PendingOperation?)null);

        var result = await _handler.HandleAsync(new RejectPendingOperationCommand { TraderId = "t1", OperationId = Guid.NewGuid() });

        Assert.False(result.IsSuccess);
        Assert.Contains("not found", result.ErrorMessage);
    }

    [Fact]
    public async Task HandleAsync_AlreadyResolved_ReturnsFailure()
    {
        var op = CreatePendingOperation("t1");
        op.Resolve();
        _pendingRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(op);

        var result = await _handler.HandleAsync(new RejectPendingOperationCommand { TraderId = "t1", OperationId = op.Id });

        Assert.False(result.IsSuccess);
        Assert.Contains("not found", result.ErrorMessage);
    }

    [Fact]
    public async Task HandleAsync_WrongTrader_ReturnsForbidden()
    {
        var op = CreatePendingOperation("trader-other");
        _pendingRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(op);

        var result = await _handler.HandleAsync(new RejectPendingOperationCommand { TraderId = "t1", OperationId = op.Id });

        Assert.False(result.IsSuccess);
        Assert.Equal("Forbidden.", result.ErrorMessage);
    }

    [Fact]
    public async Task HandleAsync_Reject_ResolvesOperation()
    {
        var op = CreatePendingOperation("t1");
        _pendingRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(op);

        var result = await _handler.HandleAsync(new RejectPendingOperationCommand { TraderId = "t1", OperationId = op.Id });

        Assert.True(result.IsSuccess);
        Assert.True(op.IsResolved);
    }

    private static PendingOperation CreatePendingOperation(string traderId)
    {
        var portfolio = new Portfolio(traderId, 100000m);
        var op = new PendingOperation(Guid.NewGuid(), portfolio.Id, Guid.NewGuid(), "Strat", "BTC", "buy", 1m, 50000m, DateTime.UtcNow.AddMinutes(5));
        typeof(PendingOperation).GetProperty("Portfolio")!.SetValue(op, portfolio);
        return op;
    }
}
