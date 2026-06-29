using TraderForge.Application.DTOs.Commands;
using TraderForge.Domain.Common;
using TraderForge.Domain.Repositories;

namespace TraderForge.Application.Handlers;

public class UnsuspendTraderCommandHandler
{
    private readonly ITraderRepository _traderRepository;

    public UnsuspendTraderCommandHandler(ITraderRepository traderRepository)
    {
        _traderRepository = traderRepository;
    }

    public async Task<Result> HandleAsync(UnsuspendTraderCommand command)
    {
        var trader = await _traderRepository.GetByIdAsync(command.TraderId);
        
        if (trader == null)
            return Result.Failure("Trader not found.");

        if (!trader.IsSuspended)
            return Result.Failure("Trader is not suspended.");

        trader.Unsuspend();
        await _traderRepository.SaveChangesAsync();

        return Result.Success();
    }
}