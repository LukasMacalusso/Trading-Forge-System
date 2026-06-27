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
        {
            return cachedCandles!;
        }

        var url = $"https://api.binance.com/api/v3/klines?symbol={symbol}&interval={interval}&limit={limit}";
        var response = await _client.GetFromJsonAsync<JsonElement[][]>(url);
        
        if (response == null) return new List<Candlestick>();

        var candles = new List<Candlestick>();
        foreach (var item in response)
        {
            candles.Add(new Candlestick(
                item[0].GetInt64(),
                decimal.Parse(item[1].GetString()!, CultureInfo.InvariantCulture),
                decimal.Parse(item[2].GetString()!, CultureInfo.InvariantCulture),
                decimal.Parse(item[3].GetString()!, CultureInfo.InvariantCulture),
                decimal.Parse(item[4].GetString()!, CultureInfo.InvariantCulture),
                decimal.Parse(item[5].GetString()!, CultureInfo.InvariantCulture),
                item[6].GetInt64()
            ));
        }
        
        _cache.Set(cacheKey, candles, TimeSpan.FromMinutes(5));
        return candles;
    }
}

public record BinancePrice(string symbol, string price);