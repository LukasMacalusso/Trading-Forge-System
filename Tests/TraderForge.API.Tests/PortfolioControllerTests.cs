using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TraderForge.API.Controllers;
using TraderForge.API.Requests;
using TraderForge.Application.DTOs;
using TraderForge.Application.Handlers;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Repositories;
using TraderForge.Domain.Services;

namespace TraderForge.API.Tests;

public class PortfolioControllerTests
{
    private readonly Mock<ITraderRepository> _traderRepoMock;
    private readonly Mock<IStrategyRepository> _strategyRepoMock;
    private readonly Mock<IPortfolioAssetRepository> _assetRepoMock;
    private readonly Mock<ISubscriptionLimitGuard> _limitGuardMock;
    private readonly PortfolioController _controller;
    private const string TraderId = "test-trader-id";

    public PortfolioControllerTests()
    {
        _traderRepoMock = new Mock<ITraderRepository>();
        _strategyRepoMock = new Mock<IStrategyRepository>();
        _assetRepoMock = new Mock<IPortfolioAssetRepository>();
        _limitGuardMock = new Mock<ISubscriptionLimitGuard>();

        var getPortfolioHandler = new GetActivePortfolioQueryHandler(_traderRepoMock.Object);
        var getStrategiesHandler = new GetStrategiesQueryHandler(_strategyRepoMock.Object);
        var getAssetsHandler = new GetPortfolioAssetsQueryHandler(_assetRepoMock.Object);
        var createStrategyHandler = new CreateStrategyCommandHandler(
            _strategyRepoMock.Object, _traderRepoMock.Object, _limitGuardMock.Object);
        var updateStrategyStateHandler = new UpdateStrategyStateCommandHandler(_strategyRepoMock.Object);
        var addAssetHandler = new AddPortfolioAssetCommandHandler(
            _assetRepoMock.Object, _traderRepoMock.Object, _limitGuardMock.Object);
        var removeAssetHandler = new RemovePortfolioAssetCommandHandler(_assetRepoMock.Object);

        _controller = new PortfolioController(
            getPortfolioHandler, getStrategiesHandler, getAssetsHandler,
            createStrategyHandler, updateStrategyStateHandler, addAssetHandler, removeAssetHandler);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, TraderId),
            new Claim(ClaimTypes.Role, "Trader")
        }, "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task GetActivePortfolio_WhenExists_ReturnsOk()
    {
        var portfolio = new Portfolio(TraderId, 10000m);
        var trader = new Trader(TraderId, "test@test.com");
        trader.Portfolios.Add(portfolio);
        _traderRepoMock.Setup(x => x.GetByIdIncludePortfolioAsync(TraderId)).ReturnsAsync(trader);

        var result = await _controller.GetActivePortfolio();
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<Portfolio>(okResult.Value);
    }

    [Fact]
    public async Task GetActivePortfolio_WhenNotFound_ReturnsNotFound()
    {
        _traderRepoMock.Setup(x => x.GetByIdIncludePortfolioAsync(It.IsAny<string>()))
            .ReturnsAsync((Trader?)null!);

        var result = await _controller.GetActivePortfolio();
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetStrategies_ReturnsOk()
    {
        var strategies = new List<Strategy> { new(Guid.NewGuid(), "S1", Guid.NewGuid()) };
        _strategyRepoMock.Setup(x => x.GetByTraderIdAsync(TraderId)).ReturnsAsync(strategies);

        var result = await _controller.GetStrategies();
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<List<Strategy>>(okResult.Value);
    }

    [Fact]
    public async Task CreateStrategy_WhenValid_ReturnsOk()
    {
        var portfolio = new Portfolio(TraderId, 10000m);
        var trader = new Trader(TraderId, "test@test.com");
        trader.Portfolios.Add(portfolio);

        _limitGuardMock.Setup(x => x.CanAddStrategyAsync(TraderId)).ReturnsAsync(true);
        _traderRepoMock.Setup(x => x.GetByIdIncludePortfolioAsync(TraderId)).ReturnsAsync(trader);

        var request = new CreateStrategyRequest { Name = "My Strategy" };
        var result = await _controller.CreateStrategy(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task CreateStrategy_WhenFails_ReturnsBadRequest()
    {
        _limitGuardMock.Setup(x => x.CanAddStrategyAsync(TraderId)).ReturnsAsync(false);

        var request = new CreateStrategyRequest { Name = "Strategy" };
        var result = await _controller.CreateStrategy(request);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UpdateStrategyState_WhenActivating_ReturnsOk()
    {
        var strategyId = Guid.NewGuid();
        var strategy = new Strategy(strategyId, "Test", Guid.NewGuid());
        strategy.Deactivate();
        _strategyRepoMock.Setup(x => x.GetByIdAsync(strategyId)).ReturnsAsync(strategy);

        var request = new UpdateStrategyStateRequest { IsActive = true };
        var result = await _controller.UpdateStrategyState(strategyId, request);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task UpdateStrategyState_WhenDeactivating_ReturnsOk()
    {
        var strategyId = Guid.NewGuid();
        var strategy = new Strategy(strategyId, "Test", Guid.NewGuid());
        _strategyRepoMock.Setup(x => x.GetByIdAsync(strategyId)).ReturnsAsync(strategy);

        var request = new UpdateStrategyStateRequest { IsActive = false };
        var result = await _controller.UpdateStrategyState(strategyId, request);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetAssets_ReturnsOk()
    {
        var assets = new List<PortfolioAsset>
        {
            new(Guid.NewGuid(), "BTCUSDT", 1m, 50000m, Guid.NewGuid()),
        };
        _assetRepoMock.Setup(x => x.GetByTraderIdAsync(TraderId)).ReturnsAsync(assets);

        var result = await _controller.GetAssets();
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<List<PortfolioAsset>>(okResult.Value);
    }

    [Fact]
    public async Task AddAsset_WhenValid_ReturnsOk()
    {
        var portfolio = new Portfolio(TraderId, 10000m);
        var trader = new Trader(TraderId, "test@test.com");
        trader.Portfolios.Add(portfolio);

        _limitGuardMock.Setup(x => x.CanAddAssetAsync(TraderId)).ReturnsAsync(true);
        _traderRepoMock.Setup(x => x.GetByIdIncludePortfolioAsync(TraderId)).ReturnsAsync(trader);

        var request = new AddPortfolioAssetRequest { Symbol = "BTCUSDT", Quantity = 1m, EntryPrice = 50000m };
        var result = await _controller.AddAsset(request);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task RemoveAsset_WhenValid_ReturnsOk()
    {
        var assetId = Guid.NewGuid();
        var asset = new PortfolioAsset(assetId, "BTCUSDT", 1m, 50000m, Guid.NewGuid());
        _assetRepoMock.Setup(x => x.GetByIdAsync(assetId)).ReturnsAsync(asset);

        var result = await _controller.RemoveAsset(assetId);
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task RemoveAsset_WhenFails_ReturnsBadRequest()
    {
        _assetRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((PortfolioAsset?)null!);

        var result = await _controller.RemoveAsset(Guid.NewGuid());
        Assert.IsType<BadRequestObjectResult>(result);
    }
}
