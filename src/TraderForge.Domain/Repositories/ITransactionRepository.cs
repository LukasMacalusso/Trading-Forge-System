using TraderForge.Domain.Entities;

namespace TraderForge.Domain.Repositories;

public interface ITransactionRepository
{
    Task<List<Transaction>> GetByPortfolioIdAsync(Guid portfolioId);
}
