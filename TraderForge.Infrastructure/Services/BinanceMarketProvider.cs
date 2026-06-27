using System.Net.Http.Json;
using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using TraderForge.Domain.Models;
using TraderForge.Domain.Services;

namespace TraderForge.Infrastructure.Services;

public class BinanceMarketProvider : IMarketDataProvider
{
    private readonly HttpClient _client;
    private readonly IMemoryCache _cache;
    private readonly string _baseUrl = "https://api.binance.com/api/v3/ticker/price";
    
    public BinanceMarketProvider(HttpClient client, IMemoryCache cache)
    {
        _client = client;
        _cache = cache;
    }

    public async Task<Dictionary<string, decimal>> GetPricesAsync()
    {
        var allPrices = await _client.GetFromJsonAsync<List<BinancePrice>>(_baseUrl);
        if (allPrices == null) return new Dictionary<string, decimal>();
        
        return allPrices.ToDictionary(
            priceSymbol => priceSymbol.symbol, 
            priceValue => decimal.Parse(priceValue.price, CultureInfo.InvariantCulture)
        );
    }
    public async Task<List<Candlestick>> GetHistoricalCandlesAsync(string symbol, string interval, int limit = 500)
    {
        var cacheKey = $"Klines_{symbol}_{interval}_{limit}";
        if (_cache.TryGetValue(cacheKey, out List<Candlestick>? cachedCandles))
            return cachedCandles!;

        var candles = await FetchAndParseCandlesAsync(symbol, interval, limit);
        
        _cache.Set(cacheKey, candles, TimeSpan.FromMinutes(5));
        return candles;
    }

    private async Task<List<Candlestick>> FetchAndParseCandlesAsync(string symbol, string interval, int limit)
    {
        var url = $"https://api.binance.com/api/v3/klines?symbol={symbol}&interval={interval}&limit={limit}";
        var response = await _client.GetFromJsonAsync<JsonElement[][]>(url);
        
        if (response == null) return new List<Candlestick>();

        return response.Select(ParseCandlestick).ToList();
    }

    private Candlestick ParseCandlestick(JsonElement[] item)
    {
        return new Candlestick(
            item[0].GetInt64(),
            ParseDecimal(item[1]),
            ParseDecimal(item[2]),
            ParseDecimal(item[3]),
            ParseDecimal(item[4]),
            ParseDecimal(item[5]),
            item[6].GetInt64()
        );
    }

    private decimal ParseDecimal(JsonElement element)
    {
        var value = element.GetString() ?? "0";
        return decimal.Parse(value, CultureInfo.InvariantCulture);
    }
}

public record BinancePrice(string symbol, string price);