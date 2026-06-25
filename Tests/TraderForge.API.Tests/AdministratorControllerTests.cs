using Microsoft.AspNetCore.Mvc;
using Moq;
using TraderForge.API.Controllers;
using TraderForge.API.Requests;
using TraderForge.Application.DTOs;
using TraderForge.Application.Handlers;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Repositories;

namespace TraderForge.API.Tests;

public class AdministratorControllerTests
{
    private readonly ISubscriptionPlanRepository _planRepo;
    private readonly AdministratorController _controller;

    public AdministratorControllerTests()
    {
        _planRepo = Mock.Of<ISubscriptionPlanRepository>();
        var getAllHandler = new GetAllPlansQueryHandler(_planRepo);
        var createHandler = new CreatePlanCommandHandler(_planRepo);
        var updateHandler = new UpdatePlanCommandHandler(_planRepo);
        var deleteHandler = new DeletePlanCommandHandler(_planRepo);

        _controller = new AdministratorController(
            getAllHandler, createHandler, updateHandler, deleteHandler, _planRepo);
    }

    [Fact]
    public async Task GetAll_WhenPlansExist_ReturnsOkWithPlans()
    {
        var plans = new List<SubscriptionPlan>
        {
            new(Guid.NewGuid(), "Basic", 9.99m, 10000m, 2, 5, false),
            new(Guid.NewGuid(), "Pro", 29.99m, 50000m, 10, 20, false),
        };
        Mock.Get(_planRepo).Setup(x => x.GetAllAsync()).ReturnsAsync(plans);

        var result = await _controller.GetAll();
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedPlans = Assert.IsType<List<SubscriptionPlan>>(okResult.Value);
        Assert.Equal(2, returnedPlans.Count);
    }

    [Fact]
    public async Task GetAll_WhenNoPlans_ReturnsOkWithEmptyList()
    {
        Mock.Get(_planRepo).Setup(x => x.GetAllAsync()).ReturnsAsync(new List<SubscriptionPlan>());

        var result = await _controller.GetAll();
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedPlans = Assert.IsType<List<SubscriptionPlan>>(okResult.Value);
        Assert.Empty(returnedPlans);
    }

    [Fact]
    public async Task GetById_WhenPlanExists_ReturnsOk()
    {
        var planId = Guid.NewGuid();
        var plan = new SubscriptionPlan(planId, "Basic", 9.99m, 10000m, 2, 5, false);
        Mock.Get(_planRepo).Setup(x => x.GetByIdAsync(planId)).ReturnsAsync(plan);

        var result = await _controller.GetById(planId);
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<SubscriptionPlan>(okResult.Value);
    }

    [Fact]
    public async Task GetById_WhenPlanNotFound_ReturnsNotFound()
    {
        Mock.Get(_planRepo).Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((SubscriptionPlan?)null!);

        var result = await _controller.GetById(Guid.NewGuid());
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task Create_WhenValid_ReturnsOk()
    {
        Mock.Get(_planRepo).Setup(x => x.AddAsync(It.IsAny<SubscriptionPlan>())).Returns(Task.CompletedTask);

        var request = new CreatePlanRequest
        { Name = "Test", MonthlyPrice = 10m, InitialVirtualBalance = 5000m, MaxActiveStrategies = 2, MaxActiveAssets = 5, CanModifyVirtualBalance = false };

        var result = await _controller.Create(request);
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Update_WhenValid_ReturnsOk()
    {
        var planId = Guid.NewGuid();
        var existingPlan = new SubscriptionPlan(planId, "Basic", 9.99m, 10000m, 2, 5, false);
        Mock.Get(_planRepo).Setup(x => x.GetByIdAsync(planId)).ReturnsAsync(existingPlan);
        Mock.Get(_planRepo).Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

        var request = new UpdatePlanRequest
        { Name = "Updated", MonthlyPrice = 15m, InitialVirtualBalance = 20000m, MaxActiveStrategies = 5, MaxActiveAssets = 10, CanModifyVirtualBalance = true };

        var result = await _controller.Update(planId, request);
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Update_WhenPlanNotFound_ReturnsBadRequest()
    {
        Mock.Get(_planRepo).Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((SubscriptionPlan?)null!);

        var request = new UpdatePlanRequest
        { Name = "Updated", MonthlyPrice = 15m, InitialVirtualBalance = 20000m, MaxActiveStrategies = 5, MaxActiveAssets = 10, CanModifyVirtualBalance = true };

        var result = await _controller.Update(Guid.NewGuid(), request);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Delete_WhenValid_ReturnsOk()
    {
        var planId = Guid.NewGuid();
        var plan = new SubscriptionPlan(planId, "Basic", 9.99m, 10000m, 2, 5, false);
        Mock.Get(_planRepo).Setup(x => x.GetByIdAsync(planId)).ReturnsAsync(plan);
        Mock.Get(_planRepo).Setup(x => x.DeleteAsync(planId)).Returns(Task.CompletedTask);

        var result = await _controller.Delete(planId);
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Delete_WhenPlanNotFound_ReturnsBadRequest()
    {
        Mock.Get(_planRepo).Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((SubscriptionPlan?)null!);

        var result = await _controller.Delete(Guid.NewGuid());
        Assert.IsType<BadRequestObjectResult>(result);
    }
}
