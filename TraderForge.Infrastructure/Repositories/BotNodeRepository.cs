using Microsoft.EntityFrameworkCore;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Repositories;
using TraderForge.Infrastructure.Persistence;

namespace TraderForge.Infrastructure.Repositories;

public class BotNodeRepository : IBotNodeRepository
{
    private readonly ApplicationDbContext _dbContext;

    public BotNodeRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<BotNode>> GetByStrategyIdAsync(Guid strategyId)
    {
        return await _dbContext.BotNodes
            .Where(n => n.StrategyId == strategyId)
            .OrderBy(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<BotNode?> GetByIdAsync(Guid id)
    {
        return await _dbContext.BotNodes.FirstOrDefaultAsync(n => n.Id == id);
    }

    public async Task AddAsync(BotNode node)
    {
        await _dbContext.BotNodes.AddAsync(node);
    }

    public void Update(BotNode node)
    {
        _dbContext.BotNodes.Update(node);
    }

    public void Remove(BotNode node)
    {
        _dbContext.BotNodes.Remove(node);
    }

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}
