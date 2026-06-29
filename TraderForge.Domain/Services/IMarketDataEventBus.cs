using TraderForge.Domain.Events;

namespace TraderForge.Domain.Services;

public interface IMarketDataEventBus
{
    void Publish(MarketPriceEvent evt);
    IAsyncEnumerable<MarketPriceEvent> ReadAllAsync(CancellationToken ct);
}
