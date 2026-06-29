using TraderForge.Domain.Common;
using TraderForge.Application.DTOs;
using TraderForge.Domain.Entities;
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
            var price = await EnsurePriceIsAvailableAsync(command.Symbol);
            await EnsureSubscriptionLimitNotReachedAsync(command.TraderId);
            var trader = await EnsureTraderExistsAsync(command.TraderId);

            ExecuteTrade(trader, command, price);
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

    private async Task<decimal> EnsurePriceIsAvailableAsync(string symbol)
    {
        if (!_marketService.IsMarketOpen(symbol))
            throw new InvalidOperationException($"The market for {symbol} is currently closed.");

        var cacheItem = await _marketService.GetPricesAsync();

        if ((DateTime.UtcNow - cacheItem.LastUpdated) > TimeSpan.FromSeconds(60))
        {
            throw new InvalidOperationException("Cannot execute trade: Market data is outdated.");
        }

        if (!cacheItem.Prices.TryGetValue(symbol, out var currentPrice))
        {
            throw new InvalidOperationException($"Current price for {symbol} is unavailable.");
        }

        return currentPrice;
    }

    private async Task EnsureSubscriptionLimitNotReachedAsync(string traderId)
    {
        var canAdd = await _limitGuard.CanAddAssetAsync(traderId);
        if (!canAdd)
            throw new InvalidOperationException("Subscription limit reached.");
    }

    private async Task<Trader> EnsureTraderExistsAsync(string traderId)
    {
        var trader = await _traderRepository.GetByIdIncludePlanAndPositionsAsync(traderId);
        if (trader == null)
            throw new InvalidOperationException("Trader not found.");

        return trader;
    }

    private void ExecuteTrade(Trader trader, BuyPositionCommand command, decimal price)
    {
        trader.BuyPosition(command.Symbol, command.Quantity, price, _commissionService);
    }

    private async Task SaveChangesAsync()
    {
        await _traderRepository.SaveChangesAsync();
    }
}
