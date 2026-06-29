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
        var traderId = "u1";
        var portfolio = new Portfolio(traderId, 10000m);
        var portfolioId = portfolio.Id;
        var trader = new Trader(traderId, "test@test.com");
        trader.Portfolios.Add(portfolio);

        var traderRepo = new Mock<ITraderRepository>();
        traderRepo.Setup(x => x.GetByIdIncludePortfolioAsync(traderId)).ReturnsAsync(trader);

        var positionRepo = new Mock<IPositionRepository>();
        positionRepo.Setup(x => x.GetByPortfolioIdAsync(portfolioId)).ReturnsAsync(new List<Position>());

        var handler = new GetPositionsQueryHandler(traderRepo.Object, positionRepo.Object);
        var result = await handler.HandleAsync(new GetPositionsQuery { TraderId = "u1" });
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
    }
}
