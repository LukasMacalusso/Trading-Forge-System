using TraderForge.Application.Common;
using TraderForge.Application.DTOs;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Repositories;
using TraderForge.Domain.Services;

namespace TraderForge.Application.Handlers;

public class AddPortfolioAssetCommandHandler
{
    private readonly IPortfolioAssetRepository _assetRepository;
    private readonly ITraderRepository _traderRepository;
    private readonly ISubscriptionLimitGuard _limitGuard;

    public AddPortfolioAssetCommandHandler(
        IPortfolioAssetRepository assetRepository,
        ITraderRepository traderRepository,
        ISubscriptionLimitGuard limitGuard)
    {
        _assetRepository = assetRepository;
        _traderRepository = traderRepository;
        _limitGuard = limitGuard;
    }

    public async Task<Result> HandleAsync(AddPortfolioAssetCommand command)
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

    private async Task<Result> ExecuteAsync(AddPortfolioAssetCommand command)
    {
        var canAdd = await _limitGuard.CanAddAssetAsync(command.TraderId);
        if (!canAdd)
            return Result.Failure("Subscription limit reached: maximum active assets exceeded.");

        var trader = await _traderRepository.GetByIdIncludePortfolioAsync(command.TraderId);
        var activePortfolio = trader?.Portfolios.FirstOrDefault(p => p.IsActive);
        if (activePortfolio == null)
            return Result.Failure("No active portfolio found.");

        var asset = new PortfolioAsset(
            Guid.NewGuid(),
            command.Symbol,
            command.Quantity,
            command.EntryPrice,
            activePortfolio.Id);

        await _assetRepository.AddAsync(asset);

        return Result.Success();
    }
}
