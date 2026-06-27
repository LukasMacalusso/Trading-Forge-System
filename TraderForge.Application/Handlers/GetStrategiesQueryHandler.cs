using TraderForge.Domain.Common;
using TraderForge.Application.DTOs;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Repositories;

namespace TraderForge.Application.Handlers;

public class GetStrategiesQueryHandler
{
    private readonly IStrategyRepository _strategyRepository;

    public GetStrategiesQueryHandler(IStrategyRepository strategyRepository)
    {
        _strategyRepository = strategyRepository;
    }

    public async Task<ResultGeneric<List<Strategy>>> HandleAsync(GetStrategiesQuery query)
    {
        try
        {
            var strategies = await _strategyRepository.GetByTraderIdAsync(query.TraderId);
            return ResultGeneric<List<Strategy>>.Success(strategies);
        }
        catch (Exception ex)
        {
            return ResultGeneric<List<Strategy>>.Failure(ex.Message);
        }
    }
}
