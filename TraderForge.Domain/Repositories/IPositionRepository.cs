using TraderForge.Domain.Entities;

namespace TraderForge.Domain.Repositories;

public interface IPositionRepository
{
    Task<Position?> GetByIdAsync(Guid id);
    Task<Position?> GetByIdWithPortfolioAsync(Guid id);
    Task<List<Position>> GetByTraderIdAsync(string traderId);
    Task AddAsync(Position asset);
    Task RemoveAsync(Position asset);
    Task SaveChangesAsync();
}
