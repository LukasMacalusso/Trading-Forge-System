using TraderForge.Application.Common;
using TraderForge.Application.DTOs;
using TraderForge.Domain.Repositories;
using TraderForge.Domain.Services;

namespace TraderForge.Application.Handlers;

public class BuyPositionCommandHandler
{
    private readonly ITraderRepository _traderRepository;
    private readonly ISubscriptionLimitGuard _limitGuard;
    private readonly ICommissionService _commissionService;
    private readonly IMarketService _marketService;

    public BuyPositionCommandHandler(
        ITraderRepository traderRepository,
        ISubscriptionLimitGuard limitGuard,
        ICommissionService commissionService,
        IMarketService marketService)
    {
        _traderRepository = traderRepository;
        _limitGuard = limitGuard;
        _commissionService = commissionService;
        _marketService = marketService;
    }

    public async Task<Result> HandleAsync(BuyPositionCommand command)
    {
        try
        {
            return await ExecuteTradeAsync(command);
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }
    }

    private async Task<Result> ExecuteTradeAsync(BuyPositionCommand command)
    {
        if (!_marketService.IsMarketOpen(command.Symbol))
            return Result.Failure($"The market for {command.Symbol} is currently closed.");

        var canAdd = await _limitGuard.CanAddAssetAsync(command.TraderId);
        if (!canAdd)
            return Result.Failure("Subscription limit reached.");

        var trader = await _traderRepository.GetByIdIncludePlanAndPositionsAsync(command.TraderId);
        if (trader == null)
            return Result.Failure("Trader not found.");

        trader.BuyPosition(command.Symbol, command.Quantity, command.EntryPrice, _commissionService);
        
        await _traderRepository.SaveChangesAsync();

        return Result.Success();
    }
}
