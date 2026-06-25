using Moq;
using TraderForge.Application.Common;
using TraderForge.Application.DTOs;
using TraderForge.Application.DTOs.Responses;
using TraderForge.Application.Handlers;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Repositories;

namespace TraderForge.Application.Tests;

public class GetTransactionsQueryHandlerTests
{
    private readonly Mock<ITraderRepository> _traderRepositoryMock;
    private readonly Mock<ITransactionRepository> _transactionRepositoryMock;
    private readonly GetTransactionsQueryHandler _handler;
    private readonly Trader _trader;
    private readonly Portfolio _portfolio;
    private const string TraderId = "trader-1";

    public GetTransactionsQueryHandlerTests()
    {
        _traderRepositoryMock = new Mock<ITraderRepository>();
        _transactionRepositoryMock = new Mock<ITransactionRepository>();

        _portfolio = new Portfolio(TraderId, 10000m);
        _trader = new Trader(TraderId, "test@test.com");
        _trader.Portfolios.Add(_portfolio);

        _traderRepositoryMock
            .Setup(x => x.GetByIdIncludePortfolioAsync(TraderId))
            .ReturnsAsync(_trader);

        _handler = new GetTransactionsQueryHandler(
            _traderRepositoryMock.Object,
            _transactionRepositoryMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenTransactionsExist_Should_Return_Mapped_Response()
    {
        var transactions = new List<Transaction>
        {
            new(_portfolio.Id, "Buy", 5000m, 10000m, 5000m, 25m, "BTC", 0.1m, 50000m),
            new(_portfolio.Id, "Sell", 3000m, 5000m, 8000m, 15m, "ETH", 1m, 3000m)
        };

        _transactionRepositoryMock
            .Setup(x => x.GetByPortfolioIdAsync(_portfolio.Id))
            .ReturnsAsync(transactions);

        var query = new GetTransactionsQuery { TraderId = TraderId };
        var result = await _handler.HandleAsync(query);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.Count);

        var first = result.Value[0];
        Assert.Equal("Buy", first.Type);
        Assert.Equal("BTC", first.Symbol);
        Assert.Equal(0.1m, first.Quantity);
        Assert.Equal(50000m, first.Price);
        Assert.Equal(25m, first.Commission);
        Assert.Equal(5000m, first.Total);
        Assert.Equal(10000m, first.BalanceBefore);
        Assert.Equal(5000m, first.BalanceAfter);

        var second = result.Value[1];
        Assert.Equal("Sell", second.Type);
        Assert.Equal("ETH", second.Symbol);
    }

    [Fact]
    public async Task HandleAsync_WhenNoTransactions_Should_Return_Empty_List()
    {
        _transactionRepositoryMock
            .Setup(x => x.GetByPortfolioIdAsync(_portfolio.Id))
            .ReturnsAsync(new List<Transaction>());

        var query = new GetTransactionsQuery { TraderId = TraderId };
        var result = await _handler.HandleAsync(query);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task HandleAsync_WhenTraderNotFound_Should_Return_Failure()
    {
        _traderRepositoryMock
            .Setup(x => x.GetByIdIncludePortfolioAsync(It.IsAny<string>()))
            .ReturnsAsync((Trader?)null);

        var query = new GetTransactionsQuery { TraderId = "non-existent" };
        var result = await _handler.HandleAsync(query);

        Assert.False(result.IsSuccess);
        Assert.Contains("not found", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task HandleAsync_WhenNoActivePortfolio_Should_Return_Failure()
    {
        _portfolio.FreezeSimulation();

        var query = new GetTransactionsQuery { TraderId = TraderId };
        var result = await _handler.HandleAsync(query);

        Assert.False(result.IsSuccess);
        Assert.Contains("portfolio", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task HandleAsync_WhenExceptionThrown_Should_Catch_And_Return_Failure()
    {
        _traderRepositoryMock
            .Setup(x => x.GetByIdIncludePortfolioAsync(It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("DB error"));

        var query = new GetTransactionsQuery { TraderId = TraderId };
        var result = await _handler.HandleAsync(query);

        Assert.False(result.IsSuccess);
    }
}
