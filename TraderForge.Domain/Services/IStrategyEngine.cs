namespace TraderForge.Domain.Services;

public interface IStrategyEngine
{
    Task StartStrategyAsync(Guid strategyId);
    Task StopStrategyAsync(Guid strategyId);
    bool IsStrategyRunning(Guid strategyId);
}
