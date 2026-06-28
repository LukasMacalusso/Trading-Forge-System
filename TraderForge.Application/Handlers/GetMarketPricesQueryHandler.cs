using TraderForge.Domain.Common;
using TraderForge.Application.DTOs.Queries;
using TraderForge.Application.DTOs.Responses;
using TraderForge.Domain.Services;

namespace TraderForge.Application.Handlers;

public class GetMarketPricesQueryHandler
{
    private readonly IMarketService _marketService;
    
    public GetMarketPricesQueryHandler(IMarketService marketService) => _marketService = marketService;
    
    public async Task<ResultGeneric<MarketPricesResponse>> HandleAsync(GetMarketPricesQuery query)
    {
        var cacheItem = await _marketService.GetPricesAsync();
    
        if (cacheItem.Prices.Count == 0) 
            return ResultGeneric<MarketPricesResponse>.Failure("No prices found.");
        
        bool isStale = (DateTime.UtcNow - cacheItem.LastUpdated) > TimeSpan.FromSeconds(60);
        
        var requestedPrices = cacheItem.Prices
            .Where(price => query.Symbols.Contains(price.Key))
            .ToDictionary(p => p.Key, p => p.Value);
        
        var response = new MarketPricesResponse
        {
            Prices = requestedPrices,
            IsStale = isStale 
        };
        
        return ResultGeneric<MarketPricesResponse>.Success(response);
    }
}