using Moq;
using TraderForge.Application.DTOs;
using TraderForge.Application.Handlers;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Repositories;
using TraderForge.Domain.Services;

namespace TraderForge.Application.Tests;

public class SellPositionCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_PositionNotFound_ReturnsFailure()
    {
        var marketMock = new Mock<IMarketService>();
        marketMock.Setup(m => m.IsMarketOpen(It.IsAny<string>())).Returns(true);
        var posRepo = new Mock<IPositionRepository>();
        posRepo.Setup(p => p.GetByIdWithPortfolioAsync(It.IsAny<Guid>())).ReturnsAsync((Position?)null);

        var handler = new SellPositionCommandHandler(posRepo.Object, new Mock<ITraderRepository>().Object, new Mock<ICommissionService>().Object, marketMock.Object);

        var result = await handler.HandleAsync(new SellPositionCommand { PositionId = Guid.NewGuid(), Quantity = 1 });
        Assert.False(result.IsSuccess);
        Assert.Equal("Position not found.", result.ErrorMessage);
    }
}
