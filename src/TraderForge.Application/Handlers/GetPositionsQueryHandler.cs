using TraderForge.Domain.Common;
using TraderForge.Application.DTOs;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Repositories;

namespace TraderForge.Application.Handlers;

public class GetPositionsQueryHandler
{
    private readonly ITraderRepository _traderRepository;
    private readonly IPositionRepository _assetRepository;

    public GetPositionsQueryHandler(ITraderRepository traderRepository, IPositionRepository assetRepository)
    {
        _traderRepository = traderRepository;
        _assetRepository = assetRepository;
    }

    public async Task<ResultGeneric<List<Position>>> HandleAsync(GetPositionsQuery query)
    {
        try
        {
            var trader = await _traderRepository.GetByIdIncludePortfolioAsync(query.TraderId);
            if (trader == null)
                return ResultGeneric<List<Position>>.Failure("Trader not found.");

            var portfolio = query.PortfolioId.HasValue
                ? trader.Portfolios.FirstOrDefault(p => p.Id == query.PortfolioId.Value)
                : trader.Portfolios.FirstOrDefault(p => p.IsActive);

            if (portfolio == null)
                return ResultGeneric<List<Position>>.Failure("Portfolio not found.");

            var assets = await _assetRepository.GetByPortfolioIdAsync(portfolio.Id);
            return ResultGeneric<List<Position>>.Success(assets);
        }
        catch (Exception ex)
        {
            return ResultGeneric<List<Position>>.Failure(ex.Message);
        }
    }
}
