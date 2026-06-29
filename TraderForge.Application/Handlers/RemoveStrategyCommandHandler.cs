using TraderForge.Domain.Common;
using TraderForge.Application.DTOs;
using TraderForge.Domain.Repositories;

namespace TraderForge.Application.Handlers;

public class RemoveStrategyCommandHandler
{
    private readonly IStrategyRepository _strategyRepository;

    public RemoveStrategyCommandHandler(IStrategyRepository strategyRepository)
    {
        _strategyRepository = strategyRepository;
    }

    public async Task<Result> HandleAsync(RemoveStrategyCommand command)
    {
        try
        {
            var strategy = await _strategyRepository.GetByIdAsync(command.StrategyId);
            if (strategy == null)
                return Result.Failure("Strategy not found.");

            _strategyRepository.Remove(strategy);
            await _strategyRepository.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception)
        {
            return Result.Failure("An unexpected error occurred.");
        }
    }
}
