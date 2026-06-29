using TraderForge.Domain.Common;
using TraderForge.Application.DTOs;
using TraderForge.Domain.Repositories;
using TraderForge.Domain.Services;

namespace TraderForge.Application.Handlers;

public class StopEngineCommandHandler
{
    private readonly IStrategyEngine _engine;

    public StopEngineCommandHandler(IStrategyEngine engine)
    {
        _engine = engine;
    }

    public async Task<Result> HandleAsync(StopEngineCommand command)
    {
        try
        {
            await _engine.StopStrategyAsync(command.StrategyId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
