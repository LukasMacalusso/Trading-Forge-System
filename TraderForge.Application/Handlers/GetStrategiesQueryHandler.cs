using TraderForge.Domain.Common;
using TraderForge.Application.DTOs;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Repositories;

namespace TraderForge.Application.Handlers;

public class GetStrategiesQueryHandler
{
    private readonly ITraderRepository _traderRepository;
    private readonly IStrategyRepository _strategyRepository;

    public GetStrategiesQueryHandler(ITraderRepository traderRepository, IStrategyRepository strategyRepository)
    {
        _traderRepository = traderRepository;
        _strategyRepository = strategyRepository;
    }

    public async Task<ResultGeneric<List<Strategy>>> HandleAsync(GetStrategiesQuery query)
    {
        try
        {
            var trader = await _traderRepository.GetByIdIncludePortfolioAsync(query.TraderId);
            if (trader == null)
                return ResultGeneric<List<Strategy>>.Failure("Trader not found.");

            var portfolio = query.PortfolioId.HasValue 
                ? trader.Portfolios.FirstOrDefault(p => p.Id == query.PortfolioId.Value)
                : trader.Portfolios.FirstOrDefault(p => p.IsActive);

            if (portfolio == null)
                return ResultGeneric<List<Strategy>>.Failure("Portfolio not found.");

            var strategies = await _strategyRepository.GetByPortfolioIdAsync(portfolio.Id);
            return ResultGeneric<List<Strategy>>.Success(strategies);
        }
        catch (Exception ex)
        {
            return ResultGeneric<List<Strategy>>.Failure(ex.Message);
        }
    }
}
