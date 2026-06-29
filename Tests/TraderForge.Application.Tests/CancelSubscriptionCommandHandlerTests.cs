using Moq;
using TraderForge.Application.DTOs;
using TraderForge.Application.Handlers;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Repositories;
using TraderForge.Domain.Services;

namespace TraderForge.Application.Tests;

public class CancelSubscriptionCommandHandlerTests
{
    private readonly Mock<ITraderRepository> _traderRepoMock;
    private readonly Mock<IDiscountService> _discountServiceMock;
    private readonly Mock<ISubscriptionPlanRepository> _planRepoMock;
    private readonly CancelSubscriptionCommandHandler _handler;
    private const string TraderId = "test-trader-id";

    public CancelSubscriptionCommandHandlerTests()
    {
        _traderRepoMock = new Mock<ITraderRepository>();
        _discountServiceMock = new Mock<IDiscountService>();
        _planRepoMock = new Mock<ISubscriptionPlanRepository>();

        _handler = new CancelSubscriptionCommandHandler(
            _traderRepoMock.Object,
            _discountServiceMock.Object,
            _planRepoMock.Object);
    }

    [Fact]
    public async Task HandleAsync_TraderNotFound_ReturnsFailure()
    {
        _traderRepoMock.Setup(x => x.GetByIdIncludeAllAsync(TraderId)).ReturnsAsync((Trader?)null);

        var command = new CancelSubscriptionCommand { TraderId = TraderId, ForceCancel = false };
        var result = await _handler.HandleAsync(command);

        Assert.False(result.IsSuccess);
        Assert.Equal("Trader not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task HandleAsync_WithRetentionOffer_DoesNotCancel_WhenForceIsFalse()
    {
        var planId = Guid.NewGuid();
        var trader = new Trader(TraderId, "test@test.com");
        var plans = new List<SubscriptionPlan> { new(planId, "Pro", 29.99m, 50000, 10, 20, false) };
        var offer = new DiscountOffer(10m, 19.99m);

        _traderRepoMock.Setup(x => x.GetByIdIncludeAllAsync(TraderId)).ReturnsAsync(trader);
        _planRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(plans);
        _discountServiceMock.Setup(x => x.GetEarlyCancellationOfferAsync(TraderId, planId)).ReturnsAsync(offer);

        var command = new CancelSubscriptionCommand { TraderId = TraderId, ForceCancel = false };
        var result = await _handler.HandleAsync(command);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value!.WasCancelled);
        Assert.NotNull(result.Value.RetentionOffer);
    }

    [Fact]
    public async Task HandleAsync_WithRetentionOffer_Cancels_WhenForceIsTrue()
    {
        var planId = Guid.NewGuid();
        var trader = new Trader(TraderId, "test@test.com");
        var plans = new List<SubscriptionPlan> { new(planId, "Pro", 29.99m, 50000, 10, 20, false) };
        var offer = new DiscountOffer(10m, 19.99m);

        _traderRepoMock.Setup(x => x.GetByIdIncludeAllAsync(TraderId)).ReturnsAsync(trader);
        _planRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(plans);
        _discountServiceMock.Setup(x => x.GetEarlyCancellationOfferAsync(TraderId, planId)).ReturnsAsync(offer);

        var command = new CancelSubscriptionCommand { TraderId = TraderId, ForceCancel = true };
        var result = await _handler.HandleAsync(command);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.WasCancelled);
        _traderRepoMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
}
