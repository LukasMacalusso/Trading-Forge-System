using TraderForge.Domain.Common;
using TraderForge.Application.DTOs;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TraderForge.Application.Handlers;

public class GetPortfolioHistoryQueryHandler
{
    private readonly ITraderRepository _traderRepository;

    public GetPortfolioHistoryQueryHandler(ITraderRepository traderRepository)
    {
        _traderRepository = traderRepository;
    }

    public async Task<ResultGeneric<List<Portfolio>>> HandleAsync(GetPortfolioHistoryQuery query)
    {
        try
        {
            var trader = await _traderRepository.GetByIdIncludePortfolioAsync(query.TraderId);
            if (trader == null)
                return ResultGeneric<List<Portfolio>>.Failure("Trader not found.");

            var history = trader.Portfolios
                .Where(p => !p.IsActive)
                .OrderByDescending(p => p.ClosedAt)
                .ToList();

            return ResultGeneric<List<Portfolio>>.Success(history);
        }
        catch (System.Exception ex)
        {
            return ResultGeneric<List<Portfolio>>.Failure(ex.Message);
        }
    }
}
