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

    [Fact]
    public void StartEngine_ShouldSetIsEngineActiveToTrue()
    {
        var strategy = new Strategy(Guid.NewGuid(), "Test", Guid.NewGuid());

        strategy.StartEngine();

        Assert.True(strategy.IsEngineActive);
    }

    [Fact]
    public void StopEngine_ShouldSetIsEngineActiveToFalse()
    {
        var strategy = new Strategy(Guid.NewGuid(), "Test", Guid.NewGuid());
        strategy.StartEngine();

        strategy.StopEngine();

        Assert.False(strategy.IsEngineActive);
    }

    [Fact]
    public void IsEngineActive_DefaultsToFalse()
    {
        var strategy = new Strategy(Guid.NewGuid(), "Test", Guid.NewGuid());

        Assert.False(strategy.IsEngineActive);
    }

    [Fact]
    public void BotNodes_IsInitializedAsEmpty()
    {
        var strategy = new Strategy(Guid.NewGuid(), "Test", Guid.NewGuid());

        Assert.NotNull(strategy.BotNodes);
        Assert.Empty(strategy.BotNodes);
    }

    [Fact]
    public void BotEdges_IsInitializedAsEmpty()
    {
        var strategy = new Strategy(Guid.NewGuid(), "Test", Guid.NewGuid());

        Assert.NotNull(strategy.BotEdges);
        Assert.Empty(strategy.BotEdges);
    }

    [Fact]
    public void CreatedAt_IsSetToUtcNow()
    {
        var before = DateTime.UtcNow;
        var strategy = new Strategy(Guid.NewGuid(), "Test", Guid.NewGuid());
        var after = DateTime.UtcNow;

        Assert.InRange(strategy.CreatedAt, before, after);
    }
}
