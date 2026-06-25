using TraderForge.Application.Common;
using TraderForge.Application.DTOs;
using TraderForge.Domain.Repositories;

namespace TraderForge.Application.Handlers;

public class ResetSimulationCommandHandler
{
    private readonly ITraderRepository _traderRepository;

    public ResetSimulationCommandHandler(ITraderRepository traderRepository)
    {
        _traderRepository = traderRepository;
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

        trader.ResetPortfolio();

        await _traderRepository.SaveChangesAsync();
        return Result.Success();
    }
}
