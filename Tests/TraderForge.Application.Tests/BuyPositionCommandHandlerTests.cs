using Moq;
using TraderForge.Application.DTOs;
using TraderForge.Application.DTOs.Responses;
using TraderForge.Application.Handlers;
using TraderForge.Domain.Constants;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Repositories;
using TraderForge.Domain.Services;

namespace TraderForge.Application.Tests;

public class BuyPositionCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_MarketClosed_ReturnsFailure()
    {
        var marketMock = new Mock<IMarketService>();
        marketMock.Setup(m => m.IsMarketOpen(It.IsAny<string>())).Returns(false);
        var handler = new BuyPositionCommandHandler(new Mock<ITraderRepository>().Object, new Mock<ISubscriptionLimitGuard>().Object, new Mock<ICommissionService>().Object, marketMock.Object);

        var result = await handler.HandleAsync(new BuyPositionCommand { TraderId = "u1", Symbol = "BTC", Quantity = 1 });
        Assert.False(result.IsSuccess);
        Assert.Contains("The market for BTC is currently closed.", result.ErrorMessage);
    }

    [Fact]
    public async Task HandleAsync_TraderNotFound_ReturnsFailure()
    {
        var marketMock = new Mock<IMarketService>();
        marketMock.Setup(m => m.IsMarketOpen(It.IsAny<string>())).Returns(true);
        
        marketMock.Setup(m => m.GetPricesAsync()).ReturnsAsync(new MarketPriceCacheItem() 
        { 
            Prices = new Dictionary<string, decimal> { { "BTC", 50000m } },
            LastUpdated = DateTime.UtcNow
        });
        
        var limitGuardMock = new Mock<ISubscriptionLimitGuard>();
        limitGuardMock.Setup(l => l.CanAddAssetAsync(It.IsAny<string>())).ReturnsAsync(true);
        var traderRepo = new Mock<ITraderRepository>();
        traderRepo.Setup(t => t.GetByIdIncludePlanAndPositionsAsync("u1")).ReturnsAsync((Trader?)null);

        var handler = new BuyPositionCommandHandler(traderRepo.Object, limitGuardMock.Object, new Mock<ICommissionService>().Object, marketMock.Object);

        var result = await handler.HandleAsync(new BuyPositionCommand { TraderId = "u1", Symbol = "BTC", Quantity = 1 });
        Assert.False(result.IsSuccess);
        Assert.Equal("Trader not found.", result.ErrorMessage);
    }
}
