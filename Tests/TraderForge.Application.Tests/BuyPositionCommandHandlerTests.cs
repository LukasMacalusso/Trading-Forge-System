using Moq;
using TraderForge.Application.Handlers;
using TraderForge.Domain.Repositories;
using TraderForge.Domain.Services;

namespace TraderForge.Application.Tests;

public class BuyPositionCommandHandlerTests
{
    private BuyPositionCommandHandler CreateHandler()
    {
        var positionRepositoryMock = new Mock<IPositionRepository>();
        var traderRepositoryMock = new Mock<ITraderRepository>();
        var orderRepositoryMock = new Mock<IOrderRepository>();
        var subscriptionLimitGuardMock = new Mock<ISubscriptionLimitGuard>();
        var commissionServiceMock = new Mock<ICommissionService>();
        var marketServiceMock = new Mock<IMarketService>();

        marketServiceMock.Setup(m => m.IsMarketOpen(It.IsAny<string>())).Returns(true);

        return new BuyPositionCommandHandler(
            traderRepositoryMock.Object,
            subscriptionLimitGuardMock.Object,
            commissionServiceMock.Object,
            marketServiceMock.Object
        );
    }
}
