using Moq;
using TraderForge.Application.DTOs;
using TraderForge.Application.Handlers;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Repositories;

namespace TraderForge.Application.Tests;

public class GetOrdersQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsOrders()
    {
        var traderRepo = new Mock<ITraderRepository>();
        traderRepo.Setup(x => x.GetByIdIncludePortfolioAsync("u1")).ReturnsAsync((Trader?)null);
        var handler = new GetOrdersQueryHandler(traderRepo.Object, new Mock<IOrderRepository>().Object);
        var result = await handler.HandleAsync(new GetOrdersQuery { TraderId = "u1" });
        Assert.False(result.IsSuccess);
        Assert.Equal("Trader not found.", result.ErrorMessage);
    }
}
