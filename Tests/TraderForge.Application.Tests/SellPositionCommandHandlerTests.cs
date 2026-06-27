using Moq;
using TraderForge.Application.Handlers;
using TraderForge.Domain.Repositories;
using TraderForge.Domain.Services;

namespace TraderForge.Application.Tests;

public class SellPositionCommandHandlerTests
{
    private SellPositionCommandHandler CreateHandler()
    {
        var positionRepositoryMock = new Mock<IPositionRepository>();
        var orderRepositoryMock = new Mock<IOrderRepository>();
        var commissionServiceMock = new Mock<ICommissionService>();
        var marketServiceMock = new Mock<IMarketService>();

        var traderRepositoryMock = new Mock<ITraderRepository>();

        marketServiceMock.Setup(m => m.IsMarketOpen(It.IsAny<string>())).Returns(true);
        marketServiceMock.Setup(m => m.GetPricesAsync()).ReturnsAsync(new Dictionary<string, decimal> { { "BTCUSDT", 50000m } });

        return new SellPositionCommandHandler(
            positionRepositoryMock.Object,
            traderRepositoryMock.Object,
            commissionServiceMock.Object,
            marketServiceMock.Object
        );
    }
}
