using Microsoft.AspNetCore.Mvc;
using Moq;
using TraderForge.API.Controllers;
using TraderForge.Application.Handlers;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Repositories;
using TraderForge.Domain.Services;

namespace TraderForge.API.Tests;

public class SubscriptionControllerNoAuthTests
{
    [Fact]
    public async Task GetSubscriptionPlans_WhenHandlerFails_ReturnsBadRequest()
    {
        var planRepoMock = new Mock<ISubscriptionPlanRepository>();
        planRepoMock.Setup(x => x.GetAllAsync()).ThrowsAsync(new Exception("Database error"));

        var getAllPlansHandler = new GetAllPlansQueryHandler(planRepoMock.Object);

        var controller = new SubscriptionController(
            new ChangeSubscriptionCommandHandler(
                Mock.Of<ITraderRepository>(),
                Mock.Of<ISubscriptionPlanRepository>(),
                Mock.Of<ISubscriptionLimitGuard>()),
            new CancelSubscriptionCommandHandler(
                Mock.Of<ITraderRepository>(),
                Mock.Of<IDiscountService>(),
                Mock.Of<ISubscriptionPlanRepository>()),
            Mock.Of<IDiscountService>(),
            getAllPlansHandler,
            new GetTraderPlanQueryHandler(Mock.Of<ITraderRepository>()));

        var result = await controller.GetSubscriptionPlans();
        Assert.IsType<BadRequestObjectResult>(result);
    }
}
