using TraderForge.Application.Common;
using TraderForge.Application.DTOs;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Interfaces;
using TraderForge.Domain.Repositories;
using TraderForge.Domain.Services;

namespace TraderForge.Application.Handlers;

public class CreateStrategyCommandHandler
{
    private readonly IStrategyRepository _strategyRepository;
    private readonly ITraderRepository _traderRepository;
    private readonly ISubscriptionLimitGuard _limitGuard;

    public CreateStrategyCommandHandler(
        IStrategyRepository strategyRepository,
        ITraderRepository traderRepository,
        ISubscriptionLimitGuard limitGuard)
    {
        _strategyRepository = strategyRepository;
        _traderRepository = traderRepository;
        _limitGuard = limitGuard;
    }

    public async Task<Result> HandleAsync(CreateStrategyCommand command)
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

    private async Task<Result> ExecuteAsync(CreateStrategyCommand command)
    {
        var canAdd = await _limitGuard.CanAddStrategyAsync(command.TraderId);
        if (!canAdd)
            return Result.Failure("Subscription limit reached: maximum active strategies exceeded.");

        var trader = await _traderRepository.GetByIdIncludePortfolioAsync(command.TraderId);
        var activePortfolio = trader?.Portfolios.FirstOrDefault(p => p.IsActive);
        if (activePortfolio == null)
            return Result.Failure("No active portfolio found.");

        var strategy = new Strategy(Guid.NewGuid(), command.Name, activePortfolio.Id);
        await _strategyRepository.AddAsync(strategy);

        return Result.Success();
    }
}
