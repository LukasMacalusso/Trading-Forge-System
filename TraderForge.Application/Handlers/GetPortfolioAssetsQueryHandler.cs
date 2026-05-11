using TraderForge.Application.Common;
using TraderForge.Application.DTOs;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Repositories;

namespace TraderForge.Application.Handlers;

public class GetPortfolioAssetsQueryHandler
{
    private readonly IPortfolioAssetRepository _assetRepository;

    public GetPortfolioAssetsQueryHandler(IPortfolioAssetRepository assetRepository)
    {
        _assetRepository = assetRepository;
    }

    public async Task<ResultGeneric<List<PortfolioAsset>>> HandleAsync(GetPortfolioAssetsQuery query)
    {
        try
        {
            var assets = await _assetRepository.GetByTraderIdAsync(query.TraderId);
            return ResultGeneric<List<PortfolioAsset>>.Success(assets);
        }
        catch (Exception ex)
        {
            return ResultGeneric<List<PortfolioAsset>>.Failure(ex.Message);
        }
    }
}
