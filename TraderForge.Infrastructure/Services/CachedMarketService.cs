using Microsoft.Extensions.Caching.Memory;
using TraderForge.Domain.Constants;
using TraderForge.Domain.Services;

namespace TraderForge.Infrastructure.Services;

public class CachedMarketService : IMarketService
{
    private readonly IMemoryCache _cache;
    
    public CachedMarketService(IMemoryCache cache) => _cache = cache;
    
    public async Task<Dictionary<string, decimal>> GetPricesAsync()
    {
        if (_cache.TryGetValue(CacheKeys.MarketPrices, out Dictionary<string, decimal> cachedPrices))
        {
            return cachedPrices;
        }

        return new Dictionary<string, decimal>();
    }
    
public bool IsMarketOpen(string symbol)
    {
        var estTime = GetCurrentEasternTime();
        
        if (IsWeekend(estTime))
        {
            return false;
        }

        return IsWithinTradingHours(estTime.TimeOfDay);
    }

    private DateTime GetCurrentEasternTime()
    {
        var estZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, estZone);
    }

    private bool IsWeekend(DateTime time)
    {
        return time.DayOfWeek == DayOfWeek.Saturday || time.DayOfWeek == DayOfWeek.Sunday;
    }

    private bool IsWithinTradingHours(TimeSpan timeOfDay)
    {
        var marketOpen = new TimeSpan(9, 30, 0);
        var marketClose = new TimeSpan(16, 0, 0);
        
        return timeOfDay >= marketOpen && timeOfDay <= marketClose;
    }

}