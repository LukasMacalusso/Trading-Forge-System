using TraderForge.Domain.Entities;

namespace TraderForge.Domain.Tests;

public class PortfolioTests
{
    [Fact]
    public void Constructor_ShouldInitializeActivePortfolio()
    {
        var traderId = Guid.NewGuid().ToString();
        var portfolio = new Portfolio(traderId, 10000m);

        Assert.True(portfolio.IsActive);
        Assert.Equal(10000m, portfolio.VirtualBalance);
        Assert.Equal(traderId, portfolio.TraderId);
        Assert.Null(portfolio.ClosedAt);
        Assert.Empty(portfolio.Strategies);
        Assert.Empty(portfolio.PortfolioAssets);
    }

    [Fact]
    public void FreezeSimulation_ShouldDeactivatePortfolio()
    {
        var portfolio = new Portfolio(Guid.NewGuid().ToString(), 10000m);

        portfolio.FreezeSimulation();

        Assert.False(portfolio.IsActive);
        Assert.NotNull(portfolio.ClosedAt);
    }
}
