using TraderForge.Application.Common;
using TraderForge.Application.DTOs;
using TraderForge.Domain.Repositories;

namespace TraderForge.Application.Handlers;

public class RemovePositionCommandHandler
{
    private readonly IPositionRepository _assetRepository;

    public RemovePositionCommandHandler(IPositionRepository assetRepository)
    {
        _assetRepository = assetRepository;
    }

    public async Task<Result> HandleAsync(RemovePositionCommand command)
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

    private async Task<Result> ExecuteAsync(RemovePositionCommand command)
    {
        var asset = await _assetRepository.GetByIdAsync(command.AssetId);
        if (asset == null)
            return Result.Failure("Portfolio asset not found.");

        await _assetRepository.RemoveAsync(asset);

        return Result.Success();
    }
}
