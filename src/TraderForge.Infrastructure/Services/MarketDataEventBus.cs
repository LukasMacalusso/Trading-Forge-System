using System.Threading.Channels;
using TraderForge.Domain.Events;
using TraderForge.Domain.Services;

namespace TraderForge.Infrastructure.Services;

public class MarketDataEventBus : IMarketDataEventBus
{
    private readonly Channel<MarketPriceEvent> _channel = Channel.CreateUnbounded<MarketPriceEvent>(
        new UnboundedChannelOptions { SingleReader = false, SingleWriter = false });

    public void Publish(MarketPriceEvent evt)
    {
        _channel.Writer.TryWrite(evt);
    }

    public IAsyncEnumerable<MarketPriceEvent> ReadAllAsync(CancellationToken ct)
    {
        return _channel.Reader.ReadAllAsync(ct);
    }
}
