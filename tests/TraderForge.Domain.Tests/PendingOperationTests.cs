using FluentAssertions;
using TraderForge.Domain.Entities;
using Xunit;

namespace TraderForge.Domain.Tests;

public class PendingOperationTests
{
    [Fact]
    public void Constructor_Initializes_Correctly()
    {
        var id = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();
        var strategyId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddMinutes(5);

        var op = new PendingOperation(id, portfolioId, strategyId, "Strat", "BTC", "BUY", 1.5m, 60000m, expiresAt);

        op.Id.Should().Be(id);
        op.PortfolioId.Should().Be(portfolioId);
        op.StrategyId.Should().Be(strategyId);
        op.StrategyName.Should().Be("Strat");
        op.Symbol.Should().Be("BTC");
        op.Action.Should().Be("BUY");
        op.Quantity.Should().Be(1.5m);
        op.CurrentPrice.Should().Be(60000m);
        op.ExpiresAt.Should().Be(expiresAt);
        op.IsResolved.Should().BeFalse();
        op.ConditionMetAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Resolve_SetsIsResolvedToTrue()
    {
        var op = new PendingOperation(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Strat", "BTC", "BUY", 1.5m, 60000m, DateTime.UtcNow);

        op.Resolve();
        op.IsResolved.Should().BeTrue();
    }
}
