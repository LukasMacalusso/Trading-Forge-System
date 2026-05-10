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

        marketServiceMock.Setup(m => m.IsMarketOpen(It.IsAny<string>())).Returns(true);

        return new SellPositionCommandHandler(
            positionRepositoryMock.Object,
            orderRepositoryMock.Object,
            commissionServiceMock.Object,
            marketServiceMock.Object
        );
    }
}
