using Moq;
using TraderForge.Application.DTOs.Queries;
using TraderForge.Application.Handlers;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Repositories;

namespace TraderForge.Application.Tests;

public class GetAllTradersQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsMappedTraderSummaries()
    {
        // Arrange
        var traderRepo = new Mock<ITraderRepository>();

        var trader1 = new Trader("user1", "user1@test.com");
        var trader2 = new Trader("user2", "user2@test.com");
        trader2.Suspend("Violation of terms");

        var plan = new SubscriptionPlan(Guid.NewGuid(), "Pro", 50, 1000, 5, 2, false);
        trader1.InitializeWithTrial(plan); // Will create an ActiveSubscription

        traderRepo.Setup(r => r.GetAllIncludeSubPlanAsync()).ReturnsAsync(new List<Trader> { trader1, trader2 });

        var handler = new GetAllTradersQueryHandler(traderRepo.Object);

        // Act
        var result = await handler.HandleAsync(new GetAllTradersQuery());

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        var list = result.Value.ToList();
        Assert.Equal(2, list.Count);

        // Assert trader 1 (active, with plan)
        Assert.Equal(trader1.Id, list[0].Id);
        Assert.Equal("user1@test.com", list[0].Email);
        Assert.False(list[0].IsSuspended);
        Assert.Equal(string.Empty, list[0].SuspensionReason);
        Assert.Equal(plan.Id.ToString(), list[0].ActivePlanId);

        // Assert trader 2 (suspended, no plan)
        Assert.Equal(trader2.Id, list[1].Id);
        Assert.Equal("user2@test.com", list[1].Email);
        Assert.True(list[1].IsSuspended);
        Assert.Equal("Violation of terms", list[1].SuspensionReason);
        Assert.Null(list[1].ActivePlanId);
    }
}
