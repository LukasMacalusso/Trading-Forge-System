using TraderForge.Domain.Entities;

namespace TraderForge.Domain.Interfaces;

public interface IStrategyRepository
{
    Task<Strategy?> GetByIdAsync(Guid id);
    Task<List<Strategy>> GetByTraderIdAsync(string traderId);
    Task AddAsync(Strategy strategy);
    Task SaveChangesAsync();
}
