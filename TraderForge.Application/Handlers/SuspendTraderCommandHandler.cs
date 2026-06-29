using TraderForge.Application.DTOs;
using TraderForge.Domain.Common;
using TraderForge.Domain.Repositories;

namespace TraderForge.Application.Handlers;

public class SuspendTraderCommandHandler
{
    private readonly ITraderRepository _traderRepository;

    public SuspendTraderCommandHandler(ITraderRepository traderRepository)
    {
        _traderRepository = traderRepository;
    }

    public async Task<Result> HandleAsync(SuspendTraderCommand command)
    {
        var trader = await _traderRepository.GetByIdAsync(command.TraderId);
        
        if (trader == null)
            return Result.Failure("Trader not found.");

        if (trader.IsSuspended)
            return Result.Failure("Trader is already suspended.");

        if (string.IsNullOrWhiteSpace(command.Reason))
            return Result.Failure("A suspension reason is required.");

        trader.Suspend(command.Reason);
        await _traderRepository.SaveChangesAsync();

        return Result.Success();
    }
}