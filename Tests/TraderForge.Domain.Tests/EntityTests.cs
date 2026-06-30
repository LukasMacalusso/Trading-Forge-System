using TraderForge.Domain.Entities;
using TraderForge.Domain.Enums;
using TraderForge.Domain.Models;

namespace TraderForge.Domain.Tests;

public class EntityTests
{
    [Fact]
    public void Order_ShouldInitialize()
    {
        var order = new Order(Guid.NewGuid(), "BTC", OrderSide.Buy, OrderType.Market, 2, 50000m, 50m, 50050m, OrderStatus.Pending);
        Assert.Equal("BTC", order.Symbol);
        Assert.Equal(OrderSide.Buy, order.Side);
        Assert.Equal(OrderType.Market, order.Type);
        Assert.Equal(1, order.Quantity);
        Assert.Equal(50000m, order.Price);
    }

    [Fact]
    public void Order_MarkAsFilled()
    {
        var order = new Order(Guid.NewGuid(), "BTC", OrderSide.Buy, OrderType.Market, 1, 50000m, 50m, 50050m, OrderStatus.Pending);
        order.MarkAsFilled();
        Assert.Equal(OrderStatus.Filled, order.Status);
    }

    [Fact]
    public void Order_Cancel()
    {
        var order = new Order(Guid.NewGuid(), "BTC", OrderSide.Buy, OrderType.Market, 1, 50000m, 50m, 50050m, OrderStatus.Pending);
        order.Cancel();
        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }

    [Fact]
    public void Candlestick_ShouldInitialize()
    {
        var candle = new Candlestick(0, 1, 2, 0.5m, 1.5m, 100, 0);
        Assert.Equal(1, candle.Open);
        Assert.Equal(1.5m, candle.Close);
    }
}
