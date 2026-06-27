using TraderForge.Domain.Common;
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
            var position = await EnsurePositionExistsAsync(command.PositionId);
            var price = await EnsurePriceIsAvailableAsync(position.Symbol);
            var trader = await EnsureTraderExistsAsync(position.Portfolio.TraderId);

            ExecuteSell(trader, position, command, price);
            await SaveChangesAsync();

            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }
    }

    private async Task<Position> EnsurePositionExistsAsync(Guid positionId)
    {
        var position = await _positionRepository.GetByIdWithPortfolioAsync(positionId);
        if (position == null)
            throw new InvalidOperationException("Position not found.");

        return position;
    }

    private async Task<decimal> EnsurePriceIsAvailableAsync(string symbol)
    {
        if (!_marketService.IsMarketOpen(symbol))
            throw new InvalidOperationException($"The market for {symbol} is currently closed.");

        var prices = await _marketService.GetPricesAsync();
        if (!prices.TryGetValue(symbol, out var currentPrice))
            throw new InvalidOperationException($"Current price for {symbol} is unavailable.");

        return currentPrice;
    }

    private async Task<Trader> EnsureTraderExistsAsync(string traderId)
    {
        var trader = await _traderRepository.GetByIdIncludePlanAndPositionsAsync(traderId);
        if (trader == null)
            throw new InvalidOperationException("Trader not found.");

        return trader;
    }

    private void ExecuteSell(Trader trader, Position position, SellPositionCommand command, decimal price)
    {
        trader.SellPosition(position.Symbol, command.Quantity, price, _commissionService);
    }

    private async Task SaveChangesAsync()
    {
        await _traderRepository.SaveChangesAsync();
    }
}
