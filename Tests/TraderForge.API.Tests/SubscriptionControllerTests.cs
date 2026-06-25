using System.Security.Claims;
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
        var getAllPlansHandler = new GetAllPlansQueryHandler(_planRepoMock.Object);
        var getTraderPlanHandler = new GetTraderPlanQueryHandler(_traderRepoMock.Object);

        _controller = new SubscriptionController(
            changeHandler, _discountServiceMock.Object, getAllPlansHandler, getTraderPlanHandler);

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
    public async Task ChangePlan_WhenValid_ReturnsOkWithDiscount()
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
        var result = await _controller.ChangePlan(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task ChangePlan_WhenHandlerFails_ReturnsBadRequest()
    {
        _traderRepoMock.Setup(x => x.GetByIdIncludeAllAsync(It.IsAny<string>()))
            .ReturnsAsync((Trader?)null!);

        var request = new ChangeSubscriptionRequest { NewPlanId = Guid.NewGuid() };
        var result = await _controller.ChangePlan(request);

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
        trader.AssignSubscriptionPlan(plan);
        var prop = typeof(Trader).GetProperty("SubscriptionPlan",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        prop!.SetValue(trader, plan);

        _traderRepoMock.Setup(x => x.GetByIdIncludeSubPlanAsync(TraderId)).ReturnsAsync(trader);

        var result = await _controller.GetTraderPlan();
        var okResult = Assert.IsType<OkObjectResult>(result);
    }
}
