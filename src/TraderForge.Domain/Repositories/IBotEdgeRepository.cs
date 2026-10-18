using TraderForge.Domain.Entities;

namespace TraderForge.Domain.Repositories;

public interface IBotEdgeRepository
{
    Task<List<BotEdge>> GetByStrategyIdAsync(Guid strategyId);
    Task<BotEdge?> GetByIdAsync(Guid id);
    Task AddAsync(BotEdge edge);
    void Remove(BotEdge edge);
    Task SaveChangesAsync();
}
