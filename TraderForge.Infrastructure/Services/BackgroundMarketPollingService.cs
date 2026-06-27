using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using TraderForge.Domain.Constants;
using TraderForge.Domain.Services;

namespace TraderForge.Infrastructure.Services;

public class BackgroundMarketPollingService : BackgroundService
{
    private readonly IMemoryCache _cache;
    private readonly IMarketDataBroadcaster _broadcaster;
    private readonly Uri _binanceWebSocketUri = new("wss://stream.binance.com:9443/ws/!ticker@arr");

    public BackgroundMarketPollingService(
        IMemoryCache cache,
        IMarketDataBroadcaster broadcaster)
    {
        _cache = cache;
        _broadcaster = broadcaster;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var ws = new ClientWebSocket();
            try
            {
                await ws.ConnectAsync(_binanceWebSocketUri, stoppingToken);
                var buffer = new byte[1024 * 64];

                while (ws.State == WebSocketState.Open && !stoppingToken.IsCancellationRequested)
                {
                    using var ms = new MemoryStream();
                    WebSocketReceiveResult result;
                    do
                    {
                        result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), stoppingToken);
                        ms.Write(buffer, 0, result.Count);
                    }
                    while (!result.EndOfMessage && !stoppingToken.IsCancellationRequested);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, stoppingToken);
                    }
                    else
                    {
                        ms.Position = 0;
                        using var reader = new StreamReader(ms, Encoding.UTF8);
                        var json = await reader.ReadToEndAsync(stoppingToken);
                        await ProcessWebSocketMessage(json, stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WebSocket Error]: {ex.Message}. Reconnecting in 5 seconds...");
                await Task.Delay(5000, stoppingToken);
            }
        }
    }

    private async Task ProcessWebSocketMessage(string json, CancellationToken stoppingToken)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind == JsonValueKind.Array)
            {
                var currentPrices = _cache.GetOrCreate(CacheKeys.MarketPrices, entry =>
                {
                    entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(2)); 
                    return new Dictionary<string, decimal>();
                });

                bool updated = false;

                foreach (var element in document.RootElement.EnumerateArray())
                {
                    if (element.TryGetProperty("s", out var symbolProp) &&
                        element.TryGetProperty("c", out var priceProp))
                    {
                        string symbol = symbolProp.GetString()!;
                        if (decimal.TryParse(priceProp.GetString(), out decimal price))
                        {
                            currentPrices![symbol] = price;
                            updated = true;
                        }
                    }
                }

                if (updated)
                {
                    _cache.Set(CacheKeys.MarketPrices, currentPrices, TimeSpan.FromMinutes(2));
                    await _broadcaster.BroadCastPricesAsync(currentPrices!, stoppingToken);
                }
            }
        }
        catch (JsonException)
        {
            // Ignore incomplete chunks
        }
    }
}