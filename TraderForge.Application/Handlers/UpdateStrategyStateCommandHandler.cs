using TraderForge.Application.Common;
using TraderForge.Application.DTOs;
using TraderForge.Domain.Interfaces;

namespace TraderForge.Application.Handlers;

public class UpdateStrategyStateCommandHandler
{
    private readonly IStrategyRepository _strategyRepository;

    public UpdateStrategyStateCommandHandler(IStrategyRepository strategyRepository)
    {
        _strategyRepository = strategyRepository;
    }

    public async Task<Result> HandleAsync(UpdateStrategyStateCommand command)
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

    private async Task<Result> ExecuteAsync(UpdateStrategyStateCommand command)
    {
        var strategy = await _strategyRepository.GetByIdAsync(command.StrategyId);
        if (strategy == null)
            return Result.Failure("Strategy not found.");

        if (command.IsActive)
            strategy.Activate();
        else
            strategy.Deactivate();

        await _strategyRepository.SaveChangesAsync();

        return Result.Success();
    }
}
