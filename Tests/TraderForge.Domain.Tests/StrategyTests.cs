using TraderForge.Domain.Entities;

namespace TraderForge.Domain.Tests;

public class StrategyTests
{
    [Fact]
    public void Constructor_ShouldCreateActiveStrategy()
    {
        var portfolioId = Guid.NewGuid();
        var strategyId = Guid.NewGuid();

        var strategy = new Strategy(strategyId, "Test Strategy", portfolioId);

        Assert.Equal(strategyId, strategy.Id);
        Assert.Equal("Test Strategy", strategy.Name);
        Assert.Equal(portfolioId, strategy.PortfolioId);
        Assert.True(strategy.IsActive);
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        var strategy = new Strategy(Guid.NewGuid(), "Test", Guid.NewGuid());

        strategy.Deactivate();

        Assert.False(strategy.IsActive);
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        var strategy = new Strategy(Guid.NewGuid(), "Test", Guid.NewGuid());
        strategy.Deactivate();

        strategy.Activate();

        Assert.True(strategy.IsActive);
    }
}
