using Moq;
using TraderForge.Application.DTOs;
using TraderForge.Application.Handlers;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Repositories;

namespace TraderForge.Application.Tests;

public class GetPositionsQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsPositions()
    {
        var repo = new Mock<IPositionRepository>();
        repo.Setup(x => x.GetByTraderIdAsync("u1")).ReturnsAsync(new List<Position>());
        var handler = new GetPositionsQueryHandler(repo.Object);
        var result = await handler.HandleAsync(new GetPositionsQuery { TraderId = "u1" });
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
    }
}
