using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using TraderForge.Domain.Constants;
using TraderForge.Domain.Events;
using TraderForge.Domain.Services;

namespace TraderForge.Infrastructure.Services;

public class BackgroundMarketPollingService : BackgroundService
{
    private readonly IMemoryCache _cache;
    private readonly IMarketDataBroadcaster _broadcaster;
    private readonly IMarketDataEventBus _eventBus;
    private readonly Uri _binanceWebSocketUri = new("wss://stream.binance.com:9443/ws/!miniTicker@arr");

    public BackgroundMarketPollingService(
        IMemoryCache cache,
        IMarketDataBroadcaster broadcaster,
        IMarketDataEventBus eventBus)
    {
        _cache = cache;
        _broadcaster = broadcaster;
        _eventBus = eventBus;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await MaintainConnectionAsync(stoppingToken);
        }
    }

    private async Task MaintainConnectionAsync(CancellationToken stoppingToken)
    {
        using var webSocket = new ClientWebSocket();
        try
        {
            await webSocket.ConnectAsync(_binanceWebSocketUri, stoppingToken);
            await ListenForMessagesAsync(webSocket, stoppingToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WebSocket Error]: {ex.Message}. Reconnecting in 5 seconds...");
            await Task.Delay(5000, stoppingToken);
        }
    }

    private async Task ListenForMessagesAsync(ClientWebSocket webSocket, CancellationToken stoppingToken)
    {
        var buffer = new byte[1024 * 64];

        while (webSocket.State == WebSocketState.Open && !stoppingToken.IsCancellationRequested)
        {
            var json = await ReceiveFullMessageAsync(webSocket, buffer, stoppingToken);
            if (json == null)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, stoppingToken);
                break;
            }

            await ProcessWebSocketMessageAsync(json, stoppingToken);
        }
    }

    private static async Task<string?> ReceiveFullMessageAsync(ClientWebSocket webSocket, byte[] buffer, CancellationToken stoppingToken)
    {
        using var memoryStream = new MemoryStream();
        WebSocketReceiveResult result;

        do
        {
            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), stoppingToken);
            await memoryStream.WriteAsync(buffer, 0, result.Count, stoppingToken);
        }
        while (!result.EndOfMessage && !stoppingToken.IsCancellationRequested);

        if (result.MessageType == WebSocketMessageType.Close)
            return null;

        memoryStream.Position = 0;
        using var reader = new StreamReader(memoryStream, Encoding.UTF8);
        return await reader.ReadToEndAsync(stoppingToken);
    }

    private async Task ProcessWebSocketMessageAsync(string json, CancellationToken stoppingToken)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind != JsonValueKind.Array)
                return;

            var currentPrices = GetOrCreatePriceCache();
            var pricesUpdated = TryUpdatePrices(document.RootElement, currentPrices.Prices);

            if (pricesUpdated)
            {
                await SaveAndBroadcastPricesAsync(currentPrices, stoppingToken);
            }
        }
        catch (JsonException)
        {
            // Ignore malformed JSON chunks
        }
    }

    private MarketPriceCacheItem GetOrCreatePriceCache()
    {
        return _cache.GetOrCreate(CacheKeys.MarketPrices, entry =>
        {
            entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(2));
            return new MarketPriceCacheItem { LastUpdated = DateTime.UtcNow };
        }) ?? new MarketPriceCacheItem();
    }

    private static bool TryUpdatePrices(JsonElement rootArray, Dictionary<string, decimal> currentPrices)
    {
        bool anyUpdated = false;

        foreach (var element in rootArray.EnumerateArray())
        {
            if (TryExtractPriceData(element, out var symbol, out var price))
            {
                currentPrices[symbol] = price;
                anyUpdated = true;
            }
        }

        return anyUpdated;
    }

    private static bool TryExtractPriceData(JsonElement element, out string symbol, out decimal price)
    {
        symbol = string.Empty;
        price = 0;

        if (!element.TryGetProperty("s", out var symbolProp) || !element.TryGetProperty("c", out var priceProp))
            return false;

        symbol = symbolProp.GetString() ?? string.Empty;
        return decimal.TryParse(priceProp.GetString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out price);
    }

    private async Task SaveAndBroadcastPricesAsync(MarketPriceCacheItem currentPrices, CancellationToken stoppingToken)
    {
        currentPrices.LastUpdated = DateTime.UtcNow;
        _cache.Set(CacheKeys.MarketPrices, currentPrices, TimeSpan.FromMinutes(2));
        await _broadcaster.BroadCastPricesAsync(currentPrices.Prices, stoppingToken);

        foreach (var (symbol, price) in currentPrices.Prices)
        {
            _eventBus.Publish(new MarketPriceEvent(symbol, price, DateTime.UtcNow));
        }
    }
}
