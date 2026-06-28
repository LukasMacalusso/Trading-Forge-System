using TraderForge.Domain.Common;
using TraderForge.Domain.Repositories;

namespace TraderForge.Application.Handlers;

public record SuspendTraderCommand(string TraderId, string Reason);

public class SuspendTraderCommandHandler
{
    private readonly ITraderRepository _traderRepository;

    public SuspendTraderCommandHandler(ITraderRepository traderRepository) => _traderRepository = traderRepository;

    public async Task<Result> HandleAsync(SuspendTraderCommand command)
    {
        var trader = await _traderRepository.GetByIdAsync(command.TraderId);
        if (trader == null) return Result.Failure("Trader no encontrado.");

        trader.Suspend(command.Reason);
        await _traderRepository.SaveChangesAsync();

        return Result.Success();
    }
}