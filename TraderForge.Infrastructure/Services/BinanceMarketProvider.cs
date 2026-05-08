using System.Net.Http.Json;
using System.Globalization;
using TraderForge.Domain.Services;

namespace TraderForge.Infrastructure.Services;

public class BinanceMarketProvider : IMarketDataProvider
{
    private readonly HttpClient _client;
    private readonly string _baseUrl = "https://api.binance.com/api/v3/ticker/price";
    
    public BinanceMarketProvider(HttpClient client) => _client = client;

    public async Task<Dictionary<string, decimal>> GetPricesAsync()
    {
        var allPrices = await _client.GetFromJsonAsync<List<BinancePrice>>(_baseUrl);
        if (allPrices == null) return new Dictionary<string, decimal>();
        
        return allPrices.ToDictionary(
            priceSymbol => priceSymbol.symbol, 
            priceValue => decimal.Parse(priceValue.price, CultureInfo.InvariantCulture)
        );
    }
}

public record BinancePrice(string symbol, string price);