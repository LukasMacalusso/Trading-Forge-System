using TraderForge.Domain.Entities;

namespace TraderForge.Domain.Repositories;

public interface IStrategyRepository
{
    Task<Strategy?> GetByIdAsync(Guid id);
    Task<Strategy?> GetByIdWithGraphAsync(Guid id);
    Task<List<Strategy>> GetByTraderIdAsync(string traderId);
    Task<List<Strategy>> GetActiveWithEngineRunningAsync();
    Task AddAsync(Strategy strategy);
    Task SaveChangesAsync();
}
