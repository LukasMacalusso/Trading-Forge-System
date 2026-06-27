using Microsoft.Extensions.Configuration;
using Moq;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Repositories;
using TraderForge.Infrastructure.Services;

namespace TraderForge.Infrastructure.Tests;

public class DiscountServiceTests
{
    private readonly Mock<ITraderRepository> _traderRepoMock;
    private readonly Mock<ISubscriptionPlanRepository> _planRepoMock;
    private readonly Mock<IConfiguration> _configMock;
    private readonly DiscountService _service;

    public DiscountServiceTests()
    {
        _traderRepoMock = new Mock<ITraderRepository>();
        _planRepoMock = new Mock<ISubscriptionPlanRepository>();
        _configMock = new Mock<IConfiguration>();
        _service = new DiscountService(_traderRepoMock.Object, _planRepoMock.Object, _configMock.Object);
    }

    [Fact]
    public async Task GetEarlyCancellationOfferAsync_WhenOnTrial_ReturnsOffer()
    {
        var traderId = Guid.NewGuid().ToString();
        var planId = Guid.NewGuid();
        var activeSub = new ActiveSubscription(traderId, planId, 7);
        typeof(ActiveSubscription).GetProperty("StartDate")!.SetValue(activeSub, DateTime.UtcNow.AddDays(-2));
        typeof(ActiveSubscription).GetProperty("EndDate")!.SetValue(activeSub, DateTime.UtcNow.AddDays(5));
        
        var trader = new Trader(traderId, "test@test.com");
        typeof(Trader).GetProperty("Subscription")!.SetValue(trader, activeSub);
        var plan = new SubscriptionPlan(planId, "Pro", 29.99m, 50000m, 10, 20, false);

        _traderRepoMock.Setup(x => x.GetByIdAsync(traderId)).ReturnsAsync(trader);
        _planRepoMock.Setup(x => x.GetByIdAsync(planId)).ReturnsAsync(plan);

        var configSectionMock = new Mock<IConfigurationSection>();
        configSectionMock.Setup(x => x.Value).Returns("15");
        _configMock.Setup(x => x.GetSection("SubscriptionSettings:EarlyCancellationDiscountPercentage"))
            .Returns(configSectionMock.Object);

        var offer = await _service.GetEarlyCancellationOfferAsync(traderId, planId);

        Assert.NotNull(offer);
        Assert.Equal(15m, offer!.Percentage);
        Assert.Equal(25.4915m, offer.DiscountedPrice);
    }

    [Fact]
    public async Task GetEarlyCancellationOfferAsync_WhenTraderNotFound_ReturnsNull()
    {
        _traderRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((Trader?)null!);

        var result = await _service.GetEarlyCancellationOfferAsync("nonexistent", Guid.NewGuid());
        Assert.Null(result);
    }

    [Fact]
    public async Task GetEarlyCancellationOfferAsync_WhenPlanNotFound_ReturnsNull()
    {
        var traderId = Guid.NewGuid().ToString();
        _traderRepoMock.Setup(x => x.GetByIdAsync(traderId))
            .ReturnsAsync(new Trader(traderId, "test@test.com"));
        _planRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((SubscriptionPlan?)null!);

        var result = await _service.GetEarlyCancellationOfferAsync(traderId, Guid.NewGuid());
        Assert.Null(result);
    }

    [Fact]
    public async Task GetEarlyCancellationOfferAsync_WhenNotOnTrial_ReturnsNull()
    {
        var traderId = Guid.NewGuid().ToString();
        var planId = Guid.NewGuid();
        var activeSub = new ActiveSubscription(traderId, planId, 30);
        typeof(ActiveSubscription).GetProperty("StartDate")!.SetValue(activeSub, DateTime.UtcNow.AddDays(-2));
        typeof(ActiveSubscription).GetProperty("EndDate")!.SetValue(activeSub, DateTime.UtcNow.AddDays(28));
        
        var trader = new Trader(traderId, "test@test.com");
        typeof(Trader).GetProperty("Subscription")!.SetValue(trader, activeSub);
        var plan = new SubscriptionPlan(planId, "Pro", 29.99m, 50000m, 10, 20, false);

        _traderRepoMock.Setup(x => x.GetByIdAsync(traderId)).ReturnsAsync(trader);
        _planRepoMock.Setup(x => x.GetByIdAsync(planId)).ReturnsAsync(plan);

        var result = await _service.GetEarlyCancellationOfferAsync(traderId, planId);
        Assert.Null(result);
    }
}
