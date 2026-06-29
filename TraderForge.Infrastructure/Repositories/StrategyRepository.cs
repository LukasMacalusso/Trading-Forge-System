using Microsoft.EntityFrameworkCore;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Repositories;
using TraderForge.Infrastructure.Persistence;

namespace TraderForge.Infrastructure.Repositories;

public class StrategyRepository : IStrategyRepository
{
    private readonly ApplicationDbContext _dbContext;

    public StrategyRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Strategy>> GetByTraderIdAsync(string traderId)
    {
        return await _dbContext.Strategies
            .Where(s => s.Portfolio.TraderId == traderId)
            .ToListAsync();
    }

    public async Task<Strategy?> GetByIdAsync(Guid id)
    {
        return await _dbContext.Strategies.FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Strategy?> GetByIdWithGraphAsync(Guid id)
    {
        return await _dbContext.Strategies
            .Include(s => s.BotNodes.OrderBy(n => n.CreatedAt))
            .Include(s => s.BotEdges)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<List<Strategy>> GetActiveWithEngineRunningAsync()
    {
        return await _dbContext.Strategies
            .Where(s => s.IsActive && s.IsEngineActive)
            .Include(s => s.BotNodes.OrderBy(n => n.CreatedAt))
            .Include(s => s.BotEdges)
            .ToListAsync();
    }

    public async Task AddAsync(Strategy strategy)
    {
        await _dbContext.Strategies.AddAsync(strategy);
        await SaveChangesAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}
