using TraderForge.Application.Common;
using TraderForge.Application.DTOs;
using TraderForge.Domain.Repositories;
using TraderForge.Domain.Services;

namespace TraderForge.Application.Handlers;

public class SellPositionCommandHandler
{
    private readonly IPositionRepository _positionRepository;
    private readonly ITraderRepository _traderRepository;
    private readonly ICommissionService _commissionService;
    private readonly IMarketService _marketService;

    public SellPositionCommandHandler(
        IPositionRepository positionRepository,
        ITraderRepository traderRepository,
        ICommissionService commissionService,
        IMarketService marketService)
    {
        _positionRepository = positionRepository;
        _traderRepository = traderRepository;
        _commissionService = commissionService;
        _marketService = marketService;
    }

    public async Task<Result> HandleAsync(SellPositionCommand command)
    {
        try
        {
            return await ExecuteSellAsync(command);
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }
    }

    private async Task<Result> ExecuteSellAsync(SellPositionCommand command)
    {
        var position = await _positionRepository.GetByIdWithPortfolioAsync(command.PositionId);
        if (position == null)
            return Result.Failure("Position not found.");

        if (!_marketService.IsMarketOpen(position.Symbol))
            return Result.Failure($"The market for {position.Symbol} is currently closed.");

        var trader = await _traderRepository.GetByIdIncludePlanAndPositionsAsync(position.Portfolio.TraderId);
        if (trader == null)
            return Result.Failure("Trader not found.");

        trader.SellPosition(position.Symbol, command.Quantity, position.EntryPrice, _commissionService);

        await _traderRepository.SaveChangesAsync();

        return Result.Success();
    }
}
