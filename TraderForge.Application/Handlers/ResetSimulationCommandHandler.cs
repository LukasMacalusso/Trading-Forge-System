using MediatR;
using TraderForge.Domain.Common;
using TraderForge.Application.DTOs;
using TraderForge.Application.Events;
using TraderForge.Domain.Repositories;

namespace TraderForge.Application.Handlers;

public class ResetSimulationCommandHandler
{
    private readonly ITraderRepository _traderRepository;
    private readonly IPublisher _publisher;

    public ResetSimulationCommandHandler(ITraderRepository traderRepository, IPublisher publisher)
    {
        _traderRepository = traderRepository;
        _publisher = publisher;
    }

    public async Task<Result> HandleAsync(ResetSimulationCommand command)
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

    private async Task<Result> ExecuteAsync(ResetSimulationCommand command)
    {
        var trader = await _traderRepository.GetByIdIncludePlanAndPositionsAsync(command.TraderId);
        if (trader == null)
            return Result.Failure("Trader not found.");

        if (trader.Subscription?.Plan == null)
            return Result.Failure("Active subscription or plan not found.");

        var balanceRestored = trader.Subscription.Plan.InitialVirtualBalance;
        trader.ResetPortfolio(balanceRestored);

        await _traderRepository.SaveChangesAsync();
        await _publisher.Publish(new SimulationResetEvent(trader.Email, trader.UserName, balanceRestored));
        return Result.Success();
    }
}
