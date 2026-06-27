using TraderForge.Domain.Common;
using TraderForge.Application.DTOs;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Repositories;

namespace TraderForge.Application.Handlers;

public class GetPositionsQueryHandler
{
    private readonly IPositionRepository _assetRepository;

    public GetPositionsQueryHandler(IPositionRepository assetRepository)
    {
        _assetRepository = assetRepository;
    }

    public async Task<ResultGeneric<List<Position>>> HandleAsync(GetPositionsQuery query)
    {
        try
        {
            var assets = await _assetRepository.GetByTraderIdAsync(query.TraderId);
            return ResultGeneric<List<Position>>.Success(assets);
        }
        catch (Exception ex)
        {
            return ResultGeneric<List<Position>>.Failure(ex.Message);
        }
    }
}
