using Moq;
using TraderForge.Application.Common;
using TraderForge.Application.DTOs;
using TraderForge.Application.Handlers;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Repositories;

namespace TraderForge.Tests;

public class AdminPlanHandlersTests
{
    private readonly Guid _basicId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private readonly Guid _proId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private SubscriptionPlan CreateBasicPlan() => new(
        _basicId, "Basic", 9.99m, 10000m, 2, 5, false);

    private SubscriptionPlan CreateProPlan() => new(
        _proId, "Pro", 29.99m, 50000m, 10, 20, false);

    [Fact]
    public async Task CreatePlanCommandHandler_Should_Create_Plan_Successfully()
    {
        var repoMock = new Mock<ISubscriptionPlanRepository>();
        repoMock.Setup(r => r.AddAsync(It.IsAny<SubscriptionPlan>())).Returns(Task.CompletedTask);

        var handler = new CreatePlanCommandHandler(repoMock.Object);
        var command = new CreatePlanCommand
        {
            Name = "TestPlan",
            MonthlyPrice = 15m,
            InitialVirtualBalance = 25000m,
            MaxActiveStrategies = 5,
            MaxActiveAssets = 10,
            CanModifyVirtualBalance = false
        };

        var result = await handler.HandleAsync(command);

        Assert.True(result.IsSuccess);
        repoMock.Verify(r => r.AddAsync(It.IsAny<SubscriptionPlan>()), Times.Once);
    }

    [Fact]
    public async Task UpdatePlanCommandHandler_Should_Update_Plan_Successfully()
    {
        var existingPlan = CreateBasicPlan();
        var repoMock = new Mock<ISubscriptionPlanRepository>();
        repoMock.Setup(r => r.GetByIdAsync(_basicId)).ReturnsAsync(existingPlan);
        repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var handler = new UpdatePlanCommandHandler(repoMock.Object);
        var command = new UpdatePlanCommand
        {
            PlanId = _basicId,
            Name = "Basic Updated",
            MonthlyPrice = 14.99m,
            InitialVirtualBalance = 20000m,
            MaxActiveStrategies = 5,
            MaxActiveAssets = 10,
            CanModifyVirtualBalance = true
        };

        var result = await handler.HandleAsync(command);

        Assert.True(result.IsSuccess);
        Assert.Equal("Basic Updated", existingPlan.Name);
        Assert.Equal(14.99m, existingPlan.MonthlyPrice);
        Assert.Equal(20000m, existingPlan.InitialVirtualBalance);
        Assert.Equal(5, existingPlan.MaxActiveStrategies);
        Assert.Equal(10, existingPlan.MaxActiveAssets);
        Assert.True(existingPlan.CanModifyVirtualBalance);
        repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdatePlanCommandHandler_Should_Fail_When_Plan_Not_Found()
    {
        var repoMock = new Mock<ISubscriptionPlanRepository>();
        repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((SubscriptionPlan)null!);

        var handler = new UpdatePlanCommandHandler(repoMock.Object);
        var command = new UpdatePlanCommand
        {
            PlanId = Guid.NewGuid(),
            Name = "Nonexistent",
            MonthlyPrice = 10m,
            InitialVirtualBalance = 5000m,
            MaxActiveStrategies = 1,
            MaxActiveAssets = 2,
            CanModifyVirtualBalance = false
        };

        var result = await handler.HandleAsync(command);

        Assert.False(result.IsSuccess);
        Assert.Equal("Subscription plan not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task DeletePlanCommandHandler_Should_Delete_Plan_Successfully()
    {
        var existingPlan = CreateBasicPlan();
        var repoMock = new Mock<ISubscriptionPlanRepository>();
        repoMock.Setup(r => r.GetByIdAsync(_basicId)).ReturnsAsync(existingPlan);
        repoMock.Setup(r => r.DeleteAsync(_basicId)).Returns(Task.CompletedTask);

        var handler = new DeletePlanCommandHandler(repoMock.Object);
        var result = await handler.HandleAsync(_basicId);

        Assert.True(result.IsSuccess);
        repoMock.Verify(r => r.DeleteAsync(_basicId), Times.Once);
    }

    [Fact]
    public async Task DeletePlanCommandHandler_Should_Fail_When_Plan_Not_Found()
    {
        var repoMock = new Mock<ISubscriptionPlanRepository>();
        repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((SubscriptionPlan)null!);

        var handler = new DeletePlanCommandHandler(repoMock.Object);
        var result = await handler.HandleAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal("Subscription plan not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetAllPlansQueryHandler_Should_Return_All_Plans()
    {
        var plans = new List<SubscriptionPlan> { CreateBasicPlan(), CreateProPlan() };
        var repoMock = new Mock<ISubscriptionPlanRepository>();
        repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(plans);

        var handler = new GetAllPlansQueryHandler(repoMock.Object);
        var result = await handler.HandleAsync(new GetAllPlansQuery());

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.Count);
    }

    [Fact]
    public async Task GetPlanById_Should_Return_Correct_Plan()
    {
        var basicPlan = CreateBasicPlan();
        var proPlan = CreateProPlan();
        var repoMock = new Mock<ISubscriptionPlanRepository>();
        repoMock.Setup(r => r.GetByIdAsync(_basicId)).ReturnsAsync(basicPlan);
        repoMock.Setup(r => r.GetByIdAsync(_proId)).ReturnsAsync(proPlan);

        var result = await repoMock.Object.GetByIdAsync(_basicId);

        Assert.NotNull(result);
        Assert.Equal(_basicId, result.Id);
        Assert.Equal("Basic", result.Name);

        var proResult = await repoMock.Object.GetByIdAsync(_proId);
        Assert.NotNull(proResult);
        Assert.Equal(_proId, proResult.Id);
        Assert.Equal("Pro", proResult.Name);
    }
}
