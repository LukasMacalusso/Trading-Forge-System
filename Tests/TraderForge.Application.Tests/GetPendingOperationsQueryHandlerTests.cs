using Moq;
using TraderForge.Application.DTOs;
using TraderForge.Application.Handlers;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Repositories;
using Xunit;

namespace TraderForge.Application.Tests;

public class GetPendingOperationsQueryHandlerTests
{
    private readonly Mock<IPendingOperationRepository> _repoMock;
    private readonly GetPendingOperationsQueryHandler _handler;

    public GetPendingOperationsQueryHandlerTests()
    {
        _repoMock = new Mock<IPendingOperationRepository>();
        _handler = new GetPendingOperationsQueryHandler(_repoMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ReturnsPendingOperations()
    {
        var portfolio = new Portfolio("trader1", 100000m);
        var ops = new List<PendingOperation>
        {
            new(Guid.NewGuid(), portfolio.Id, Guid.NewGuid(), "Strat1", "BTC", "buy", 1m, 50000m, DateTime.UtcNow.AddMinutes(5)),
            new(Guid.NewGuid(), portfolio.Id, Guid.NewGuid(), "Strat2", "ETH", "sell", 2m, 3000m, DateTime.UtcNow.AddMinutes(3))
        };

        _repoMock.Setup(x => x.GetPendingByTraderIdAsync("trader1")).ReturnsAsync(ops);

        var result = await _handler.HandleAsync(new GetPendingOperationsQuery { TraderId = "trader1" });

        Assert.True(result.IsSuccess);
        var list = result.Value!.ToList();
        Assert.Equal(2, list.Count);
    }

    [Fact]
    public async Task HandleAsync_NoPending_ReturnsEmpty()
    {
        _repoMock.Setup(x => x.GetPendingByTraderIdAsync("trader1")).ReturnsAsync(new List<PendingOperation>());

        var result = await _handler.HandleAsync(new GetPendingOperationsQuery { TraderId = "trader1" });

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
    }
}
