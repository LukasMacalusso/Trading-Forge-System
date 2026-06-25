using TraderForge.Domain.Entities;

namespace TraderForge.Domain.Tests;

public class PortfolioTests
{
    private readonly Guid _portfolioId = Guid.NewGuid();
    private const string TraderId = "trader-1";
    private const decimal InitialBalance = 10000m;

    private Portfolio CreatePortfolio() => new(TraderId, InitialBalance);

    [Fact]
    public void Constructor_Should_Set_Initial_State()
    {
        var portfolio = CreatePortfolio();

        Assert.Equal(TraderId, portfolio.TraderId);
        Assert.Equal(InitialBalance, portfolio.VirtualBalance);
        Assert.True(portfolio.IsActive);
        Assert.NotNull(portfolio.Transactions);
        Assert.Empty(portfolio.Transactions);
    }

    [Fact]
    public void DeductFunds_Should_Reduce_Balance_And_Add_Transaction()
    {
        var portfolio = CreatePortfolio();
        var total = 500m;
        var commission = 5m;

        portfolio.DeductFunds(total, "Buy", "BTC", 0.1m, 5000m, commission);

        Assert.Equal(InitialBalance - total, portfolio.VirtualBalance);
        var transaction = Assert.Single(portfolio.Transactions);
        Assert.Equal("Buy", transaction.Type);
        Assert.Equal("BTC", transaction.Symbol);
        Assert.Equal(0.1m, transaction.Quantity);
        Assert.Equal(5000m, transaction.Price);
        Assert.Equal(commission, transaction.Commission);
        Assert.Equal(total, transaction.Total);
        Assert.Equal(InitialBalance, transaction.BalanceBefore);
        Assert.Equal(InitialBalance - total, transaction.BalanceAfter);
    }

    [Fact]
    public void DeductFunds_Should_Freeze_When_Balance_Reaches_Zero()
    {
        var portfolio = CreatePortfolio();

        portfolio.DeductFunds(InitialBalance, "Buy", "BTC", 1m, 10000m, 0m);

        Assert.Equal(0, portfolio.VirtualBalance);
        Assert.False(portfolio.IsActive);
        Assert.NotNull(portfolio.ClosedAt);
    }

    [Fact]
    public void DeductFunds_Should_Freeze_When_Balance_Goes_Negative()
    {
        var portfolio = CreatePortfolio();

        portfolio.DeductFunds(InitialBalance + 1, "Buy", "BTC", 1m, 10001m, 0m);

        Assert.True(portfolio.VirtualBalance < 0);
        Assert.False(portfolio.IsActive);
        Assert.NotNull(portfolio.ClosedAt);
    }

    [Fact]
    public void DeductFunds_Should_Not_Freeze_When_Balance_Remains_Positive()
    {
        var portfolio = CreatePortfolio();

        portfolio.DeductFunds(100m, "Buy", "BTC", 0.01m, 10000m, 0m);

        Assert.Equal(InitialBalance - 100m, portfolio.VirtualBalance);
        Assert.True(portfolio.IsActive);
        Assert.Null(portfolio.ClosedAt);
    }

    [Fact]
    public void AddFunds_Should_Increase_Balance_And_Add_Transaction()
    {
        var portfolio = CreatePortfolio();
        var total = 5000m;
        var commission = 25m;

        portfolio.AddFunds(total, "Sell", "BTC", 1m, 5000m, commission);

        Assert.Equal(InitialBalance + total, portfolio.VirtualBalance);
        var transaction = Assert.Single(portfolio.Transactions);
        Assert.Equal("Sell", transaction.Type);
        Assert.Equal("BTC", transaction.Symbol);
        Assert.Equal(1m, transaction.Quantity);
        Assert.Equal(5000m, transaction.Price);
        Assert.Equal(commission, transaction.Commission);
        Assert.Equal(total, transaction.Total);
        Assert.Equal(InitialBalance, transaction.BalanceBefore);
        Assert.Equal(InitialBalance + total, transaction.BalanceAfter);
    }

    [Fact]
    public void AddFunds_Should_Not_Freeze_Portfolio()
    {
        var portfolio = CreatePortfolio();

        portfolio.AddFunds(1000m, "Sell", "ETH", 1m, 1000m, 0m);

        Assert.True(portfolio.IsActive);
        Assert.Null(portfolio.ClosedAt);
    }

    [Fact]
    public void FreezeSimulation_Should_Mark_Portfolio_Inactive()
    {
        var portfolio = CreatePortfolio();

        portfolio.FreezeSimulation();

        Assert.False(portfolio.IsActive);
        Assert.NotNull(portfolio.ClosedAt);
    }

    [Fact]
    public void Multiple_Transactions_Should_Be_Tracked_In_Order()
    {
        var portfolio = CreatePortfolio();

        portfolio.DeductFunds(100m, "Buy", "BTC", 0.01m, 10000m, 5m);
        portfolio.AddFunds(200m, "Sell", "ETH", 0.1m, 2000m, 10m);
        portfolio.DeductFunds(50m, "Buy", "SOL", 1m, 50m, 2.5m);

        Assert.Equal(3, portfolio.Transactions.Count);
        Assert.Equal(InitialBalance - 100m + 200m - 50m, portfolio.VirtualBalance);
        Assert.Equal(InitialBalance, portfolio.Transactions.ElementAt(0).BalanceBefore);
        Assert.Equal(InitialBalance - 100m, portfolio.Transactions.ElementAt(1).BalanceBefore);
        Assert.Equal(InitialBalance - 100m + 200m, portfolio.Transactions.ElementAt(2).BalanceBefore);
    }
}
