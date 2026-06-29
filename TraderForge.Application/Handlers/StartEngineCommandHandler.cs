using TraderForge.Domain.Common;
using TraderForge.Application.DTOs;
using TraderForge.Domain.Repositories;
using TraderForge.Domain.Services;

namespace TraderForge.Application.Handlers;

public class StartEngineCommandHandler
{
    private readonly IStrategyRepository _strategyRepository;
    private readonly IStrategyEngine _engine;

    public StartEngineCommandHandler(IStrategyRepository strategyRepository, IStrategyEngine engine)
    {
        _strategyRepository = strategyRepository;
        _engine = engine;
    }

    public async Task<Result> HandleAsync(StartEngineCommand command)
    {
        try
        {
            var strategy = await _strategyRepository.GetByIdWithGraphAsync(command.StrategyId);
            if (strategy == null)
                return Result.Failure("Strategy not found.");
            if (!strategy.IsActive)
                return Result.Failure("Strategy is not active.");

            var hasTrigger = strategy.BotNodes.Any(n => n.Type == Domain.Enums.BotNodeType.Trigger);
            if (!hasTrigger)
                return Result.Failure("Strategy must have at least one Trigger node.");

            await _engine.StartStrategyAsync(command.StrategyId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
