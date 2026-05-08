using Microsoft.AspNetCore.SignalR;
using TraderForge.API.Hubs;
using TraderForge.Domain.Services;
namespace TraderForge.API.Services;
public class SignalRMarketDataBroadcaster : IMarketDataBroadcaster
{
    private readonly IHubContext<MarketDataHub> _hubContext;
    public SignalRMarketDataBroadcaster(IHubContext<MarketDataHub> hubContext)
    {
        _hubContext = hubContext;
    }
    public async Task BroadCastPricesAsync(Dictionary<string, decimal> prices, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.All.SendAsync("ReceivePriceUpdate", prices, cancellationToken);
    }
}
