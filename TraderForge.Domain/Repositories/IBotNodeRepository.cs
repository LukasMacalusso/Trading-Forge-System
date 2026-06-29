using TraderForge.Domain.Entities;

namespace TraderForge.Domain.Repositories;

public interface IBotNodeRepository
{
    Task<List<BotNode>> GetByStrategyIdAsync(Guid strategyId);
    Task<BotNode?> GetByIdAsync(Guid id);
    Task AddAsync(BotNode node);
    void Update(BotNode node);
    void Remove(BotNode node);
    Task SaveChangesAsync();
}
