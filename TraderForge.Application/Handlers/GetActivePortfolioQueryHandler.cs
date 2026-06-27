using TraderForge.Domain.Common;
using TraderForge.Application.DTOs;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Repositories;

namespace TraderForge.Application.Handlers;

public class GetActivePortfolioQueryHandler
{
    private readonly ITraderRepository _traderRepository;

    public GetActivePortfolioQueryHandler(ITraderRepository traderRepository)
    {
        _traderRepository = traderRepository;
    }

    public async Task<ResultGeneric<Portfolio>> HandleAsync(GetActivePortfolioQuery query)
    {
        try
        {
            var trader = await _traderRepository.GetByIdIncludePortfolioAsync(query.TraderId);
            if (trader == null)
                return ResultGeneric<Portfolio>.Failure("Trader not found.");

            var activePortfolio = trader.Portfolios.FirstOrDefault(p => p.IsActive);
            if (activePortfolio == null)
                return ResultGeneric<Portfolio>.Failure("No active portfolio found.");

            return ResultGeneric<Portfolio>.Success(activePortfolio);
        }
        catch (Exception ex)
        {
            return ResultGeneric<Portfolio>.Failure(ex.Message);
        }
    }
}
