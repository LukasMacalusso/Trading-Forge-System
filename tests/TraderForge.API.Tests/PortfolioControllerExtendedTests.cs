using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TraderForge.API.Controllers;
using TraderForge.Domain.Repositories;
using TraderForge.API.Mappers;
using TraderForge.API.Requests;
using TraderForge.Application.DTOs;
using TraderForge.Application.Handlers;
using TraderForge.Domain.Common;
using TraderForge.Domain.Constants;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Services;

namespace TraderForge.API.Tests;

public class PortfolioControllerExtendedTests
{
    private readonly Mock<ITraderRepository> _traderRepo;
    private readonly Mock<IStrategyRepository> _strategyRepo;
    private readonly Mock<IPositionRepository> _positionRepo;
    private readonly Mock<ITransactionRepository> _transactionRepo;
    private readonly Mock<IOrderRepository> _orderRepo;
    private readonly Mock<ISubscriptionLimitGuard> _limitGuard;
    private readonly Mock<ICommissionService> _commissionService;
    private readonly Mock<IMarketService> _marketService;
    private readonly Mock<IPendingOperationRepository> _pendingRepo;
    private readonly PortfolioController _controller;
    private readonly string _traderId = "test-trader-id";

    public PortfolioControllerExtendedTests()
    {
        _traderRepo = new Mock<ITraderRepository>();
        _strategyRepo = new Mock<IStrategyRepository>();
        _positionRepo = new Mock<IPositionRepository>();
        _transactionRepo = new Mock<ITransactionRepository>();
        _orderRepo = new Mock<IOrderRepository>();
        _limitGuard = new Mock<ISubscriptionLimitGuard>();
        _commissionService = new Mock<ICommissionService>();
        _marketService = new Mock<IMarketService>();
        _pendingRepo = new Mock<IPendingOperationRepository>();

        _marketService.Setup(m => m.IsMarketOpen(It.IsAny<string>())).Returns(true);
        _marketService.Setup(m => m.GetPricesAsync()).ReturnsAsync(new MarketPriceCacheItem
        {
            Prices = new Dictionary<string, decimal> { ["BTCUSDT"] = 50000m },
            LastUpdated = DateTime.UtcNow
        });

        _strategyRepo.Setup(r => r.GetByPortfolioIdAsync(It.IsAny<Guid>())).ReturnsAsync(new List<Strategy>());
        _positionRepo.Setup(r => r.GetByPortfolioIdAsync(It.IsAny<Guid>())).ReturnsAsync(new List<Position>());
        _transactionRepo.Setup(r => r.GetByPortfolioIdAsync(It.IsAny<Guid>())).ReturnsAsync(new List<Transaction>());
        _orderRepo.Setup(r => r.GetByPortfolioIdAsync(It.IsAny<Guid>())).ReturnsAsync(new List<Order>());

        var getPortfolioHandler = new GetActivePortfolioQueryHandler(_traderRepo.Object);
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
        var getPortfolioHistoryHandler = new GetPortfolioHistoryQueryHandler(_traderRepo.Object);
        var getPendingHandler = new GetPendingOperationsQueryHandler(_pendingRepo.Object);
        var approvePendingHandler = new ApprovePendingOperationCommandHandler(_pendingRepo.Object, _commissionService.Object);
        var rejectPendingHandler = new RejectPendingOperationCommandHandler(_pendingRepo.Object);

        _controller = new PortfolioController(
            getPortfolioHandler, getPortfolioHistoryHandler, getStrategiesHandler, getPositionsHandler,
            createStrategyHandler, buyPositionHandler, sellPositionHandler,
            getTransactionsHandler, getOrdersHandler, resetSimulationHandler,
            getPendingHandler, approvePendingHandler, rejectPendingHandler);

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

    private static Trader CreateTraderWithPortfolioAndPlan(string traderId)
    {
        var trader = new Trader(traderId, "test@test.com");
        trader.Portfolios.Add(new Portfolio(traderId, 10000m));
        var plan = new SubscriptionPlan(Guid.NewGuid(), "Pro", 29.99m, 50000m, null, null, false);
        var sub = new ActiveSubscription(traderId, plan.Id, 30);
        typeof(ActiveSubscription).GetProperty("Plan")!.SetValue(sub, plan);
        typeof(Trader).GetProperty("Subscription")!.SetValue(trader, sub);
        return trader;
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
            new ResetSimulationCommandHandler(_traderRepo.Object, Mock.Of<IPublisher>()),
            new GetPendingOperationsQueryHandler(_pendingRepo.Object),
            new ApprovePendingOperationCommandHandler(_pendingRepo.Object, _commissionService.Object),
            new RejectPendingOperationCommandHandler(_pendingRepo.Object));
        ctrl.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
        };
        return ctrl;
    }

    [Fact]
    public async Task GetActivePortfolio_Success_ReturnsOk()
    {
        var trader = CreateTraderWithPortfolio(_traderId);
        _traderRepo.Setup(r => r.GetByIdIncludePortfolioAsync(_traderId)).ReturnsAsync(trader);

        var result = await _controller.GetActivePortfolio();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task GetActivePortfolio_NotFound_ReturnsNotFound()
    {
        _traderRepo.Setup(r => r.GetByIdIncludePortfolioAsync(_traderId)).ReturnsAsync((Trader?)null);

        var result = await _controller.GetActivePortfolio();

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetActivePortfolio_NoClaim_ReturnsUnauthorized()
    {
        var ctrl = CreateControllerWithoutClaim();
        var result = await ctrl.GetActivePortfolio();
        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task GetStrategies_Success_ReturnsOk()
    {
        var trader = CreateTraderWithPortfolio(_traderId);
        _traderRepo.Setup(r => r.GetByIdIncludePortfolioAsync(_traderId)).ReturnsAsync(trader);

        var result = await _controller.GetStrategies(null);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task GetStrategies_NoClaim_ReturnsUnauthorized()
    {
        var ctrl = CreateControllerWithoutClaim();
        var result = await ctrl.GetStrategies(null);
        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task GetPositions_Success_ReturnsOk()
    {
        var trader = CreateTraderWithPortfolio(_traderId);
        _traderRepo.Setup(r => r.GetByIdIncludePortfolioAsync(_traderId)).ReturnsAsync(trader);

        var result = await _controller.GetPositions(null);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task GetPositions_NoClaim_ReturnsUnauthorized()
    {
        var ctrl = CreateControllerWithoutClaim();
        var result = await ctrl.GetPositions(null);
        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task BuyPosition_Success_ReturnsOk()
    {
        var trader = CreateTraderWithPortfolio(_traderId);
        _limitGuard.Setup(l => l.CanAddAssetAsync(_traderId)).ReturnsAsync(true);
        _traderRepo.Setup(r => r.GetByIdIncludePlanAndPositionsAsync(_traderId)).ReturnsAsync(trader);

        var result = await _controller.BuyPosition(new BuyPositionRequest { Symbol = "BTCUSDT", Quantity = 0.01m });

        var ok = Assert.IsType<OkObjectResult>(result);
        var okValue = ok.Value;
        Assert.NotNull(okValue);
        var msgProp = okValue.GetType().GetProperty("message");
        var msg = msgProp?.GetValue(okValue);
        Assert.Equal("Position purchased successfully.", msg);
    }

    [Fact]
    public async Task BuyPosition_Fails_ReturnsBadRequest()
    {
        _limitGuard.Setup(l => l.CanAddAssetAsync(_traderId)).ReturnsAsync(true);
        _traderRepo.Setup(r => r.GetByIdIncludePlanAndPositionsAsync(_traderId)).ReturnsAsync((Trader?)null);

        var result = await _controller.BuyPosition(new BuyPositionRequest { Symbol = "BTCUSDT", Quantity = 0.01m });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task BuyPosition_NoClaim_ReturnsUnauthorized()
    {
        var ctrl = CreateControllerWithoutClaim();
        var result = await ctrl.BuyPosition(new BuyPositionRequest());
        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task GetTransactions_Success_ReturnsOk()
    {
        var trader = CreateTraderWithPortfolio(_traderId);
        _traderRepo.Setup(r => r.GetByIdIncludePortfolioAsync(_traderId)).ReturnsAsync(trader);

        var result = await _controller.GetTransactions(null);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task GetOrders_Success_ReturnsOk()
    {
        var trader = CreateTraderWithPortfolio(_traderId);
        _traderRepo.Setup(r => r.GetByIdIncludePortfolioAsync(_traderId)).ReturnsAsync(trader);

        var result = await _controller.GetOrders(null);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task GetPortfolioHistory_Success_ReturnsOk()
    {
        var trader = CreateTraderWithPortfolio(_traderId);
        _traderRepo.Setup(r => r.GetByIdIncludePortfolioAsync(_traderId)).ReturnsAsync(trader);

        var result = await _controller.GetPortfolioHistory();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task ResetSimulation_Success_ReturnsOk()
    {
        var trader = CreateTraderWithPortfolioAndPlan(_traderId);
        _traderRepo.Setup(r => r.GetByIdIncludePlanAndPositionsAsync(_traderId)).ReturnsAsync(trader);

        var result = await _controller.ResetSimulation();

        var ok = Assert.IsType<OkObjectResult>(result);
        var okValue = ok.Value;
        Assert.NotNull(okValue);
        var msgProp = okValue.GetType().GetProperty("message");
        var msg = msgProp?.GetValue(okValue);
        Assert.Equal("Simulation reset successfully.", msg);
    }

    [Fact]
    public async Task GetPendingOperations_Success_ReturnsOk()
    {
        _pendingRepo.Setup(r => r.GetPendingByTraderIdAsync(_traderId)).ReturnsAsync(new List<PendingOperation>());

        var result = await _controller.GetPendingOperations();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task ApprovePendingOperation_Success_ReturnsOk()
    {
        var portfolio = new Portfolio(_traderId, 100000m);
        var op = new PendingOperation(Guid.NewGuid(), portfolio.Id, Guid.NewGuid(), "Strat", "BTC", "buy", 1m, 50000m, DateTime.UtcNow.AddMinutes(5));
        typeof(PendingOperation).GetProperty("Portfolio")!.SetValue(op, portfolio);
        _pendingRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(op);

        var result = await _controller.ApprovePendingOperation(Guid.NewGuid());

        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task RejectPendingOperation_Success_ReturnsOk()
    {
        var portfolio = new Portfolio(_traderId, 100000m);
        var op = new PendingOperation(Guid.NewGuid(), portfolio.Id, Guid.NewGuid(), "Strat", "BTC", "buy", 1m, 50000m, DateTime.UtcNow.AddMinutes(5));
        typeof(PendingOperation).GetProperty("Portfolio")!.SetValue(op, portfolio);
        _pendingRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(op);

        var result = await _controller.RejectPendingOperation(Guid.NewGuid());

        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task SellPosition_Success_ReturnsOk()
    {
        var trader = CreateTraderWithPortfolio(_traderId);
        var portfolio = trader.Portfolios.First();
        var position = new Position(Guid.NewGuid(), "BTCUSDT", 1m, 50000m, portfolio.Id);
        typeof(Position).GetProperty("Portfolio")?.SetValue(position, portfolio);
        portfolio.Positions.Add(position);
        _positionRepo.Setup(r => r.GetByIdWithPortfolioAsync(position.Id)).ReturnsAsync(position);
        _traderRepo.Setup(r => r.GetByIdIncludePlanAndPositionsAsync(_traderId)).ReturnsAsync(trader);

        var result = await _controller.SellPosition(position.Id, new SellPositionRequest { Quantity = 0.5m });

        var ok = Assert.IsType<OkObjectResult>(result);
        var okValue = ok.Value;
        Assert.NotNull(okValue);
        var msgProp = okValue.GetType().GetProperty("message");
        var msg = msgProp?.GetValue(okValue);
        Assert.Equal("Position sold successfully.", msg);
    }

    [Fact]
    public async Task SellPosition_Fails_ReturnsBadRequest()
    {
        _positionRepo.Setup(r => r.GetByIdWithPortfolioAsync(It.IsAny<Guid>())).ReturnsAsync((Position?)null);

        var result = await _controller.SellPosition(Guid.NewGuid(), new SellPositionRequest { Quantity = 0.5m });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetStrategies_HandlerFails_ReturnsBadRequest()
    {
        _traderRepo.Setup(r => r.GetByIdIncludePortfolioAsync(_traderId)).ReturnsAsync((Trader?)null);

        var result = await _controller.GetStrategies(null);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetPositions_HandlerFails_ReturnsBadRequest()
    {
        _traderRepo.Setup(r => r.GetByIdIncludePortfolioAsync(_traderId)).ReturnsAsync((Trader?)null);

        var result = await _controller.GetPositions(null);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetTransactions_HandlerFails_ReturnsBadRequest()
    {
        _traderRepo.Setup(r => r.GetByIdIncludePortfolioAsync(_traderId)).ReturnsAsync((Trader?)null);

        var result = await _controller.GetTransactions(null);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetOrders_HandlerFails_ReturnsBadRequest()
    {
        _traderRepo.Setup(r => r.GetByIdIncludePortfolioAsync(_traderId)).ReturnsAsync((Trader?)null);

        var result = await _controller.GetOrders(null);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetPortfolioHistory_HandlerFails_ReturnsBadRequest()
    {
        _traderRepo.Setup(r => r.GetByIdIncludePortfolioAsync(_traderId)).ReturnsAsync((Trader?)null);

        var result = await _controller.GetPortfolioHistory();

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task ResetSimulation_HandlerFails_ReturnsBadRequest()
    {
        _traderRepo.Setup(r => r.GetByIdIncludePlanAndPositionsAsync(_traderId)).ReturnsAsync((Trader?)null);

        var result = await _controller.ResetSimulation();

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetTransactions_NoClaim_ReturnsUnauthorized()
    {
        var ctrl = CreateControllerWithoutClaim();
        var result = await ctrl.GetTransactions(null);
        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task GetOrders_NoClaim_ReturnsUnauthorized()
    {
        var ctrl = CreateControllerWithoutClaim();
        var result = await ctrl.GetOrders(null);
        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task GetPortfolioHistory_NoClaim_ReturnsUnauthorized()
    {
        var ctrl = CreateControllerWithoutClaim();
        var result = await ctrl.GetPortfolioHistory();
        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task ResetSimulation_NoClaim_ReturnsUnauthorized()
    {
        var ctrl = CreateControllerWithoutClaim();
        var result = await ctrl.ResetSimulation();
        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task GetPendingOperations_NoClaim_ReturnsUnauthorized()
    {
        var ctrl = CreateControllerWithoutClaim();
        var result = await ctrl.GetPendingOperations();
        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task ApprovePendingOperation_NoClaim_ReturnsUnauthorized()
    {
        var ctrl = CreateControllerWithoutClaim();
        var result = await ctrl.ApprovePendingOperation(Guid.NewGuid());
        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task RejectPendingOperation_NoClaim_ReturnsUnauthorized()
    {
        var ctrl = CreateControllerWithoutClaim();
        var result = await ctrl.RejectPendingOperation(Guid.NewGuid());
        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task GetPendingOperations_HandlerThrows_PropagatesException()
    {
        _pendingRepo.Setup(r => r.GetPendingByTraderIdAsync(_traderId)).ThrowsAsync(new Exception("fail"));

        var ex = await Assert.ThrowsAsync<Exception>(() => _controller.GetPendingOperations());
        Assert.Equal("fail", ex.Message);
    }

    [Fact]
    public async Task ApprovePendingOperation_HandlerFails_ReturnsBadRequest()
    {
        _pendingRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((PendingOperation?)null);

        var result = await _controller.ApprovePendingOperation(Guid.NewGuid());

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task RejectPendingOperation_HandlerFails_ReturnsBadRequest()
    {
        _pendingRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((PendingOperation?)null);

        var result = await _controller.RejectPendingOperation(Guid.NewGuid());

        Assert.IsType<BadRequestObjectResult>(result);
    }
}
