using FluentAssertions;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Enums;
using Xunit;

namespace TraderForge.Domain.Tests;

public class OrderTests
{
    [Fact]
    public void Constructor_InitializesProperties_Correctly()
    {
        var portfolioId = Guid.NewGuid();
        var symbol = "AAPL";
        var side = OrderSide.Buy;
        var type = OrderType.Market;
        var quantity = 10m;
        var price = 150m;
        var commission = 1m;
        var total = 1501m;

        var order = new Order(portfolioId, symbol, side, type, quantity, price, commission, total, OrderStatus.Pending);

        order.Id.Should().NotBeEmpty();
        order.PortfolioId.Should().Be(portfolioId);
        order.Symbol.Should().Be(symbol);
        order.Side.Should().Be(side);
        order.Type.Should().Be(type);
        order.Quantity.Should().Be(quantity);
        order.Price.Should().Be(price);
        order.Commission.Should().Be(commission);
        order.Total.Should().Be(total);
        order.Status.Should().Be(OrderStatus.Pending);
        order.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        order.FilledAt.Should().BeNull();
    }

    [Fact]
    public void Constructor_SetsFilledAt_WhenStatusIsFilled()
    {
        var order = new Order(Guid.NewGuid(), "AAPL", OrderSide.Buy, OrderType.Market, 10, 150, 1, 1501, OrderStatus.Filled);
        order.FilledAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void MarkAsFilled_UpdatesStatusAndFilledAt()
    {
        var order = new Order(Guid.NewGuid(), "AAPL", OrderSide.Buy, OrderType.Market, 10, 150, 1, 1501, OrderStatus.Pending);

        order.MarkAsFilled();

        order.Status.Should().Be(OrderStatus.Filled);
        order.FilledAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Cancel_UpdatesStatusToCancelled()
    {
        var order = new Order(Guid.NewGuid(), "AAPL", OrderSide.Buy, OrderType.Market, 10, 150, 1, 1501, OrderStatus.Pending);

        order.Cancel();

        order.Status.Should().Be(OrderStatus.Cancelled);
    }
}
