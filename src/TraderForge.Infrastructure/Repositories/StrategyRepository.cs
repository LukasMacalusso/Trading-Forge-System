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

    public async Task<List<Strategy>> GetByPortfolioIdAsync(Guid portfolioId)
    {
        return await _dbContext.Strategies
            .Where(s => s.PortfolioId == portfolioId)
            .ToListAsync();
    }

    public async Task<Strategy?> GetByIdAsync(Guid id)
    {
        return await _dbContext.Strategies.Include(s => s.Portfolio).FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Strategy?> GetByIdWithGraphAsync(Guid id)
    {
        return await _dbContext.Strategies
            .Include(s => s.BotNodes.OrderBy(n => n.CreatedAt))
            .Include(s => s.BotEdges)
            .Include(s => s.Portfolio)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<List<Strategy>> GetActiveWithEngineRunningAsync()
    {
        return await _dbContext.Strategies
            .Where(s => s.IsActive && s.IsEngineActive && s.Portfolio.IsActive)
            .Include(s => s.BotNodes.OrderBy(n => n.CreatedAt))
            .Include(s => s.BotEdges)
            .ToListAsync();
    }

    public async Task AddAsync(Strategy strategy)
    {
        await _dbContext.Strategies.AddAsync(strategy);
        await SaveChangesAsync();
    }

    public void Remove(Strategy strategy)
    {
        _dbContext.Strategies.Remove(strategy);
    }

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}
