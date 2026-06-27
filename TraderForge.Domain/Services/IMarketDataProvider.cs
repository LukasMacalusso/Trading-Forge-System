using TraderForge.Domain.Models;

namespace TraderForge.Domain.Services;

public interface IMarketDataProvider
{
    Task<Dictionary<string, decimal>> GetPricesAsync();
    Task<List<Candlestick>> GetHistoricalCandlesAsync(string symbol, string interval, int limit = 500);
}