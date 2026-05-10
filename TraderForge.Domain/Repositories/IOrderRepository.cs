using TraderForge.Domain.Entities;

namespace TraderForge.Domain.Repositories;

public interface IOrderRepository
{
    Task AddAsync(Order order);
    Task<List<Order>> GetByPortfolioIdAsync(Guid portfolioId);
    Task SaveChangesAsync();
}
