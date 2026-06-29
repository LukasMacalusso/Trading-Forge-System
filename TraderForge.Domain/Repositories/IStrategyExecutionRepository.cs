using TraderForge.Domain.Entities;

namespace TraderForge.Domain.Repositories;

public interface IStrategyExecutionRepository
{
    Task<StrategyExecution?> GetActiveByStrategyIdAsync(Guid strategyId);
    Task AddAsync(StrategyExecution execution);
    void Update(StrategyExecution execution);
    Task SaveChangesAsync();
}
