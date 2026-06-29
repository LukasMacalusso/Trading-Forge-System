using Microsoft.EntityFrameworkCore;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Repositories;
using TraderForge.Infrastructure.Persistence;

namespace TraderForge.Infrastructure.Repositories;

public class BotEdgeRepository : IBotEdgeRepository
{
    private readonly ApplicationDbContext _dbContext;

    public BotEdgeRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<BotEdge>> GetByStrategyIdAsync(Guid strategyId)
    {
        return await _dbContext.BotEdges
            .Where(e => e.StrategyId == strategyId)
            .ToListAsync();
    }

    public async Task<BotEdge?> GetByIdAsync(Guid id)
    {
        return await _dbContext.BotEdges.FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task AddAsync(BotEdge edge)
    {
        await _dbContext.BotEdges.AddAsync(edge);
    }

    public void Remove(BotEdge edge)
    {
        _dbContext.BotEdges.Remove(edge);
    }

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}
