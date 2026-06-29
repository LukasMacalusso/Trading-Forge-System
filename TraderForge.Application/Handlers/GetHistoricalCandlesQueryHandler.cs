using TraderForge.Application.DTOs.Queries;
using TraderForge.Domain.Common;
using TraderForge.Domain.Models;
using TraderForge.Domain.Services;

namespace TraderForge.Application.Handlers;

public class GetHistoricalCandlesQueryHandler
{
    private readonly IMarketDataProvider _provider;

    public GetHistoricalCandlesQueryHandler(IMarketDataProvider provider)
    {
        _provider = provider;
    }

    public async Task<ResultGeneric<List<Candlestick>>> HandleAsync(GetHistoricalCandlesQuery query)
    {
        try
        {
            var candles = await _provider.GetHistoricalCandlesAsync(query.Symbol, query.Interval, query.Limit);
            return ResultGeneric<List<Candlestick>>.Success(candles);
        }
        catch (Exception ex)
        {
            return ResultGeneric<List<Candlestick>>.Failure($"Failed to fetch historical data: {ex.Message}");
        }
    }
}
