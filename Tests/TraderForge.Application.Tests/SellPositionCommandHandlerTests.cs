using Moq;
using TraderForge.Application.DTOs;
using TraderForge.Application.Handlers;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Repositories;
using TraderForge.Domain.Services;
using TraderForge.Domain.Constants;
using System.Reflection;

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

    [Fact]
    public async Task HandleAsync_MarketClosed_ReturnsFailure()
    {
        var marketMock = new Mock<IMarketService>();
        marketMock.Setup(m => m.IsMarketOpen(It.IsAny<string>())).Returns(false);
        
        var posRepo = new Mock<IPositionRepository>();
        var trader = new Trader("u1", "test@test.com");
        trader.InitializeWithTrial(new SubscriptionPlan(Guid.NewGuid(), "Free", 100m, 1000m, 10, 1, false));
        var portfolio = trader.Portfolios.First();
        var position = new Position(Guid.NewGuid(), "BTC", 2, 50000m, portfolio.Id);
        typeof(Position).GetProperty("Portfolio", BindingFlags.Public | BindingFlags.Instance)!.SetValue(position, portfolio);
        posRepo.Setup(p => p.GetByIdWithPortfolioAsync(It.IsAny<Guid>())).ReturnsAsync(position);

        var handler = new SellPositionCommandHandler(posRepo.Object, new Mock<ITraderRepository>().Object, new Mock<ICommissionService>().Object, marketMock.Object);

        var result = await handler.HandleAsync(new SellPositionCommand { PositionId = Guid.NewGuid(), Quantity = 1 });
        Assert.False(result.IsSuccess);
        Assert.Equal("The market for BTC is currently closed.", result.ErrorMessage);
    }

    [Fact]
    public async Task HandleAsync_PriceUnavailable_ReturnsFailure()
    {
        var marketMock = new Mock<IMarketService>();
        marketMock.Setup(m => m.IsMarketOpen(It.IsAny<string>())).Returns(true);
        marketMock.Setup(m => m.GetPricesAsync()).ReturnsAsync(new MarketPriceCacheItem() 
        { 
            Prices = new Dictionary<string, decimal>(), // Empty prices
            LastUpdated = DateTime.UtcNow
        });
        
        var posRepo = new Mock<IPositionRepository>();
        var trader = new Trader("u1", "test@test.com");
        trader.InitializeWithTrial(new SubscriptionPlan(Guid.NewGuid(), "Free", 100m, 1000m, 10, 1, false));
        var portfolio = trader.Portfolios.First();
        var position = new Position(Guid.NewGuid(), "BTC", 2, 50000m, portfolio.Id);
        typeof(Position).GetProperty("Portfolio", BindingFlags.Public | BindingFlags.Instance)!.SetValue(position, portfolio);
        posRepo.Setup(p => p.GetByIdWithPortfolioAsync(It.IsAny<Guid>())).ReturnsAsync(position);

        var handler = new SellPositionCommandHandler(posRepo.Object, new Mock<ITraderRepository>().Object, new Mock<ICommissionService>().Object, marketMock.Object);

        var result = await handler.HandleAsync(new SellPositionCommand { PositionId = Guid.NewGuid(), Quantity = 1 });
        Assert.False(result.IsSuccess);
        Assert.Equal("Current price for BTC is unavailable.", result.ErrorMessage);
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
        
        var posRepo = new Mock<IPositionRepository>();
        var trader = new Trader("u1", "test@test.com");
        trader.InitializeWithTrial(new SubscriptionPlan(Guid.NewGuid(), "Free", 100m, 1000m, 10, 1, false));
        var portfolio = trader.Portfolios.First();
        var position = new Position(Guid.NewGuid(), "BTC", 2, 50000m, portfolio.Id);
        typeof(Position).GetProperty("Portfolio", BindingFlags.Public | BindingFlags.Instance)!.SetValue(position, portfolio);
        posRepo.Setup(p => p.GetByIdWithPortfolioAsync(It.IsAny<Guid>())).ReturnsAsync(position);

        var traderRepo = new Mock<ITraderRepository>();
        traderRepo.Setup(t => t.GetByIdIncludePlanAndPositionsAsync(It.IsAny<string>())).ReturnsAsync((Trader?)null);

        var handler = new SellPositionCommandHandler(posRepo.Object, traderRepo.Object, new Mock<ICommissionService>().Object, marketMock.Object);

        var result = await handler.HandleAsync(new SellPositionCommand { PositionId = Guid.NewGuid(), Quantity = 1 });
        Assert.False(result.IsSuccess);
        Assert.Equal("Trader not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task HandleAsync_Success()
    {
        var marketMock = new Mock<IMarketService>();
        marketMock.Setup(m => m.IsMarketOpen(It.IsAny<string>())).Returns(true);
        marketMock.Setup(m => m.GetPricesAsync()).ReturnsAsync(new MarketPriceCacheItem() 
        { 
            Prices = new Dictionary<string, decimal> { { "BTC", 60000m } }, // Profit
            LastUpdated = DateTime.UtcNow
        });
        
        var posRepo = new Mock<IPositionRepository>();
        var trader = new Trader("u1", "test@test.com");
        var plan = new SubscriptionPlan(Guid.NewGuid(), "Free", 100m, 1000m, 10, 1, false);
        trader.InitializeWithTrial(plan);
        var portfolio = trader.Portfolios.First();
        portfolio.AddFunds(100000m, "Deposit", null, null, null, 0m); // Add funds
        
        // Setup initial position manually via buy to ensure it's in portfolio
        trader.BuyPosition("BTC", 2, 50000m, new Mock<ICommissionService>().Object);
        var position = portfolio.Positions.First();
        typeof(Position).GetProperty("Portfolio", BindingFlags.Public | BindingFlags.Instance)!.SetValue(position, portfolio);
        
        posRepo.Setup(p => p.GetByIdWithPortfolioAsync(It.IsAny<Guid>())).ReturnsAsync(position);

        var traderRepo = new Mock<ITraderRepository>();
        traderRepo.Setup(t => t.GetByIdIncludePlanAndPositionsAsync(trader.Id)).ReturnsAsync(trader);

        var handler = new SellPositionCommandHandler(posRepo.Object, traderRepo.Object, new Mock<ICommissionService>().Object, marketMock.Object);

        var result = await handler.HandleAsync(new SellPositionCommand { PositionId = position.Id, Quantity = 1 });
        Assert.True(result.IsSuccess, result.ErrorMessage);
        traderRepo.Verify(t => t.SaveChangesAsync(), Times.Once);
    }
}
