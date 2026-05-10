using TraderForge.Application.Common;
using TraderForge.Application.DTOs;
using TraderForge.Domain.Interfaces;

namespace TraderForge.Application.Handlers;

public class RemovePortfolioAssetCommandHandler
{
    private readonly IPortfolioAssetRepository _assetRepository;

    public RemovePortfolioAssetCommandHandler(IPortfolioAssetRepository assetRepository)
    {
        _assetRepository = assetRepository;
    }

    public async Task<Result> HandleAsync(RemovePortfolioAssetCommand command)
    {
        try
        {
            return await ExecuteAsync(command);
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }
    }

    private async Task<Result> ExecuteAsync(RemovePortfolioAssetCommand command)
    {
        var asset = await _assetRepository.GetByIdAsync(command.AssetId);
        if (asset == null)
            return Result.Failure("Portfolio asset not found.");

        await _assetRepository.RemoveAsync(asset);

        return Result.Success();
    }
}
