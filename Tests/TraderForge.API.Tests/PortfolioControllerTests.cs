using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TraderForge.API.Controllers;
using TraderForge.API.Mappers;
using TraderForge.API.Requests;
using TraderForge.Application.DTOs;
using TraderForge.Application.Handlers;
using TraderForge.Domain.Common;
using TraderForge.Domain.Constants;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Repositories;
using TraderForge.Domain.Services;

namespace TraderForge.API.Tests;

public class PortfolioControllerTests
{
    private readonly Mock<ITraderRepository> _traderRepo;
    private readonly Mock<IStrategyRepository> _strategyRepo;
    private readonly Mock<IPositionRepository> _positionRepo;
    private readonly Mock<ITransactionRepository> _transactionRepo;
    private readonly Mock<IOrderRepository> _orderRepo;
    private readonly Mock<ISubscriptionLimitGuard> _limitGuard;
    private readonly Mock<ICommissionService> _commissionService;
    private readonly Mock<IMarketService> _marketService;
    private readonly PortfolioController _controller;
    private readonly string _traderId = "test-trader-id";

    public PortfolioControllerTests()
    {
        _traderRepo = new Mock<ITraderRepository>();
        _strategyRepo = new Mock<IStrategyRepository>();
        _positionRepo = new Mock<IPositionRepository>();
        _transactionRepo = new Mock<ITransactionRepository>();
        _orderRepo = new Mock<IOrderRepository>();
        _limitGuard = new Mock<ISubscriptionLimitGuard>();
        _commissionService = new Mock<ICommissionService>();
        _marketService = new Mock<IMarketService>();

        _marketService.Setup(m => m.IsMarketOpen(It.IsAny<string>())).Returns(true);
        _marketService.Setup(m => m.GetPricesAsync()).ReturnsAsync(new MarketPriceCacheItem
        {
            Prices = new Dictionary<string, decimal> { ["BTCUSDT"] = 50000m },
            LastUpdated = DateTime.UtcNow
        });

        var getPortfolioHandler = new GetActivePortfolioQueryHandler(_traderRepo.Object);
        var getPortfolioHistoryHandler = new GetPortfolioHistoryQueryHandler(_traderRepo.Object);
        var getStrategiesHandler = new GetStrategiesQueryHandler(_traderRepo.Object, _strategyRepo.Object);
        var getPositionsHandler = new GetPositionsQueryHandler(_traderRepo.Object, _positionRepo.Object);
        var createStrategyHandler = new CreateStrategyCommandHandler(
            _strategyRepo.Object, _traderRepo.Object, _limitGuard.Object);
        var buyPositionHandler = new BuyPositionCommandHandler(
            _traderRepo.Object, _limitGuard.Object, _commissionService.Object, _marketService.Object);
        var sellPositionHandler = new SellPositionCommandHandler(
            _positionRepo.Object, _traderRepo.Object, _commissionService.Object, _marketService.Object);
        var getTransactionsHandler = new GetTransactionsQueryHandler(_traderRepo.Object, _transactionRepo.Object);
        var getOrdersHandler = new GetOrdersQueryHandler(_traderRepo.Object, _orderRepo.Object);
        var resetSimulationHandler = new ResetSimulationCommandHandler(_traderRepo.Object, Mock.Of<IPublisher>());

        _controller = new PortfolioController(
            getPortfolioHandler,
            getPortfolioHistoryHandler,
            getStrategiesHandler,
            getPositionsHandler,
            createStrategyHandler,
            buyPositionHandler,
            sellPositionHandler,
            getTransactionsHandler,
            getOrdersHandler,
            resetSimulationHandler);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, _traderId),
        }, "testAuth"));
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    private static Trader CreateTraderWithPortfolio(string traderId)
    {
        var trader = new Trader(traderId, "test@test.com");
        trader.Portfolios.Add(new Portfolio(traderId, 10000m));
        return trader;
    }

    // --- CreateStrategy ---

    [Fact]
    public async Task CreateStrategy_Success_ReturnsOk()
    {
        var trader = CreateTraderWithPortfolio(_traderId);
        _limitGuard.Setup(l => l.CanAddStrategyAsync(_traderId)).ReturnsAsync(true);
        _traderRepo.Setup(r => r.GetByIdIncludePortfolioAsync(_traderId)).ReturnsAsync(trader);

        var result = await _controller.CreateStrategy(new CreateStrategyRequest { Name = "New Strategy" });

        var ok = Assert.IsType<OkObjectResult>(result);
        var id = ok.Value.GetType().GetProperty("id")!.GetValue(ok.Value);
        var msg = ok.Value.GetType().GetProperty("message")!.GetValue(ok.Value);
        Assert.NotEqual(Guid.Empty, id);
        Assert.Equal("Strategy created successfully.", msg);
    }

    [Fact]
    public async Task CreateStrategy_HandlerFails_ReturnsBadRequest()
    {
        _limitGuard.Setup(l => l.CanAddStrategyAsync(It.IsAny<string>())).ReturnsAsync(false);

        var result = await _controller.CreateStrategy(new CreateStrategyRequest { Name = "Strategy" });

        var bad = Assert.IsType<BadRequestObjectResult>(result);
        var error = bad.Value.GetType().GetProperty("error")!.GetValue(bad.Value);
        Assert.Contains("maximum active strategies exceeded", ((string)error!));
    }

    [Fact]
    public async Task CreateStrategy_NoClaim_ReturnsUnauthorized()
    {
        var controller = CreateControllerWithoutClaim();
        var result = await controller.CreateStrategy(new CreateStrategyRequest { Name = "S" });

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    private PortfolioController CreateControllerWithoutClaim()
    {
        var ctrl = new PortfolioController(
            new GetActivePortfolioQueryHandler(_traderRepo.Object),
            new GetPortfolioHistoryQueryHandler(_traderRepo.Object),
            new GetStrategiesQueryHandler(_traderRepo.Object, _strategyRepo.Object),
            new GetPositionsQueryHandler(_traderRepo.Object, _positionRepo.Object),
            new CreateStrategyCommandHandler(_strategyRepo.Object, _traderRepo.Object, _limitGuard.Object),
            new BuyPositionCommandHandler(_traderRepo.Object, _limitGuard.Object, _commissionService.Object, _marketService.Object),
            new SellPositionCommandHandler(_positionRepo.Object, _traderRepo.Object, _commissionService.Object, _marketService.Object),
            new GetTransactionsQueryHandler(_traderRepo.Object, _transactionRepo.Object),
            new GetOrdersQueryHandler(_traderRepo.Object, _orderRepo.Object),
            new ResetSimulationCommandHandler(_traderRepo.Object, Mock.Of<IPublisher>()));
        ctrl.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
        };
        return ctrl;
    }
}
