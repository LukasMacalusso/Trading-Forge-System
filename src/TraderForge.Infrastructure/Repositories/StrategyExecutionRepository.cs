using Microsoft.EntityFrameworkCore;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Repositories;
using TraderForge.Infrastructure.Persistence;

namespace TraderForge.Infrastructure.Repositories;

public class StrategyExecutionRepository : IStrategyExecutionRepository
{
    private readonly ApplicationDbContext _dbContext;

    public StrategyExecutionRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<StrategyExecution?> GetActiveByStrategyIdAsync(Guid strategyId)
    {
        return await _dbContext.StrategyExecutions
            .Where(e => e.StrategyId == strategyId && e.Status == Domain.Enums.ExecutionStatus.Running)
            .FirstOrDefaultAsync();
    }

    public async Task AddAsync(StrategyExecution execution)
    {
        await _dbContext.StrategyExecutions.AddAsync(execution);
    }

    public void Update(StrategyExecution execution)
    {
        _dbContext.StrategyExecutions.Update(execution);
    }

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}
