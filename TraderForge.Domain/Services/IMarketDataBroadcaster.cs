using TraderForge.Domain.Entities;

namespace TraderForge.Domain.Services;

public interface IMarketDataBroadcaster
{
    Task BroadCastPricesAsync(Dictionary<string,decimal> prices, CancellationToken cancellationToken = default);
}