using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TraderForge.API.Controllers;
using TraderForge.API.Requests;
using TraderForge.Application.Handlers;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Repositories;
using TraderForge.Domain.Services;

namespace TraderForge.API.Tests;

public class SubscriptionControllerTests
{
    private readonly Mock<ITraderRepository> _traderRepoMock;
    private readonly Mock<ISubscriptionPlanRepository> _planRepoMock;
    private readonly Mock<ISubscriptionLimitGuard> _limitGuardMock;
    private readonly Mock<IDiscountService> _discountServiceMock;
    private readonly SubscriptionController _controller;
    private const string TraderId = "test-trader-id";

    public SubscriptionControllerTests()
    {
        _traderRepoMock = new Mock<ITraderRepository>();
        _planRepoMock = new Mock<ISubscriptionPlanRepository>();
        _limitGuardMock = new Mock<ISubscriptionLimitGuard>();
        _discountServiceMock = new Mock<IDiscountService>();

        var changeHandler = new ChangeSubscriptionCommandHandler(
            _traderRepoMock.Object, _planRepoMock.Object, _limitGuardMock.Object);
        
        var cancelHandler = new CancelSubscriptionCommandHandler(
            _traderRepoMock.Object, _discountServiceMock.Object, _planRepoMock.Object, Mock.Of<IPublisher>());
            
        var getAllPlansHandler = new GetAllPlansQueryHandler(_planRepoMock.Object);
        var getTraderPlanHandler = new GetTraderPlanQueryHandler(_traderRepoMock.Object);

        _controller = new SubscriptionController(
            changeHandler, cancelHandler, _discountServiceMock.Object, getAllPlansHandler, getTraderPlanHandler);

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
    public async Task ProcessPayment_WhenValid_ReturnsOkWithDiscount()
    {
        var planId = Guid.NewGuid();
        var trader = new Trader(TraderId, "test@test.com");
        var newPlan = new SubscriptionPlan(planId, "Pro", 29.99m, 50000m, 10, 20, false);

        _traderRepoMock.Setup(x => x.GetByIdIncludeAllAsync(TraderId)).ReturnsAsync(trader);
        _planRepoMock.Setup(x => x.GetByIdAsync(planId)).ReturnsAsync(newPlan);
        _limitGuardMock.Setup(x => x.CanSwitchToPlanAsync(TraderId, newPlan)).ReturnsAsync(true);
        _discountServiceMock
            .Setup(x => x.GetEarlyCancellationOfferAsync(TraderId, planId))
            .ReturnsAsync(new DiscountOffer(10m, 26.99m));

        var request = new ChangeSubscriptionRequest { NewPlanId = planId, PromoCode = "SAVE10" };
        var result = await _controller.ProcessPayment(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task ProcessPayment_WhenHandlerFails_ReturnsBadRequest()
    {
        _traderRepoMock.Setup(x => x.GetByIdIncludeAllAsync(It.IsAny<string>()))
            .ReturnsAsync((Trader?)null!);

        var request = new ChangeSubscriptionRequest { NewPlanId = Guid.NewGuid() };
        var result = await _controller.ProcessPayment(request);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetSubscriptionPlans_ReturnsOk()
    {
        var plans = new List<SubscriptionPlan>
        {
            new(Guid.NewGuid(), "Basic", 9.99m, 10000m, 2, 5, false),
        };
        _planRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(plans);

        var result = await _controller.GetSubscriptionPlans();
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedPlans = Assert.IsType<List<SubscriptionPlan>>(okResult.Value);
        Assert.Single(returnedPlans);
    }

    [Fact]
    public async Task GetSubscriptionPlans_WhenHandlerFails_ReturnsBadRequest()
    {
        _planRepoMock.Setup(x => x.GetAllAsync()).ThrowsAsync(new Exception("Database error"));

        var result = await _controller.GetSubscriptionPlans();
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetTraderPlan_ReturnsOk()
    {
        var planId = Guid.NewGuid();
        var plan = new SubscriptionPlan(planId, "Basic", 9.99m, 10000m, 2, 5, false);
        var trader = new Trader(TraderId, "test@test.com");
        trader.InitializeWithTrial(plan);
        var prop = typeof(Trader).GetProperty("Subscription",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        prop!.SetValue(trader, new ActiveSubscription(TraderId, planId, 7));

        _traderRepoMock.Setup(x => x.GetByIdIncludeSubPlanAsync(TraderId)).ReturnsAsync(trader);

        var result = await _controller.GetTraderPlan();
        var okResult = Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task CancelSubscription_WhenForceCancelTrue_ReturnsOk()
    {
        var planId = Guid.NewGuid();
        var trader = new Trader(TraderId, "test@test.com");
        var plans = new List<SubscriptionPlan> { new(planId, "Pro", 29.99m, 50000, 10, 20, false) };
        var offer = new DiscountOffer(10m, 19.99m);

        _traderRepoMock.Setup(x => x.GetByIdIncludeAllAsync(TraderId)).ReturnsAsync(trader);
        _planRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(plans);
        _discountServiceMock.Setup(x => x.GetEarlyCancellationOfferAsync(TraderId, planId)).ReturnsAsync(offer);

        var result = await _controller.CancelSubscription(forceCancel: true);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var value = okResult.Value as dynamic;
        Assert.NotNull(value);
    }

    [Fact]
    public async Task CancelSubscription_WhenOfferAvailableAndForceCancelFalse_ReturnsRetentionOffer()
    {
        var planId = Guid.NewGuid();
        var trader = new Trader(TraderId, "test@test.com");
        var plans = new List<SubscriptionPlan> { new(planId, "Pro", 29.99m, 50000, 10, 20, false) };
        var offer = new DiscountOffer(10m, 19.99m);

        _traderRepoMock.Setup(x => x.GetByIdIncludeAllAsync(TraderId)).ReturnsAsync(trader);
        _planRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(plans);
        _discountServiceMock.Setup(x => x.GetEarlyCancellationOfferAsync(TraderId, planId)).ReturnsAsync(offer);

        var result = await _controller.CancelSubscription(forceCancel: false);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var value = okResult.Value as dynamic;
        Assert.NotNull(value);
    }
}
