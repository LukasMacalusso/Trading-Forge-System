using TraderForge.Domain.Events;

namespace TraderForge.Domain.Tests;

public class MarketPriceEventTests
{
    [Fact]
    public void Constructor_SetsAllProperties()
    {
        var timestamp = new DateTime(2026, 6, 28, 12, 0, 0, DateTimeKind.Utc);
        var evt = new MarketPriceEvent("BTCUSDT", 65432.10m, timestamp);

        Assert.Equal("BTCUSDT", evt.Symbol);
        Assert.Equal(65432.10m, evt.Price);
        Assert.Equal(timestamp, evt.Timestamp);
    }

    [Fact]
    public void Deconstruct_ReturnsComponents()
    {
        var timestamp = DateTime.UtcNow;
        var evt = new MarketPriceEvent("ETHUSDT", 3500.50m, timestamp);

        var (symbol, price, ts) = evt;

        Assert.Equal("ETHUSDT", symbol);
        Assert.Equal(3500.50m, price);
        Assert.Equal(timestamp, ts);
    }

    [Fact]
    public void Equality_SameValues_AreEqual()
    {
        var ts = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var a = new MarketPriceEvent("SOLUSDT", 145.25m, ts);
        var b = new MarketPriceEvent("SOLUSDT", 145.25m, ts);

        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var ts = DateTime.UtcNow;
        var a = new MarketPriceEvent("BTCUSDT", 50000m, ts);
        var b = new MarketPriceEvent("ETHUSDT", 50000m, ts);

        Assert.NotEqual(a, b);
        Assert.True(a != b);
    }
}
