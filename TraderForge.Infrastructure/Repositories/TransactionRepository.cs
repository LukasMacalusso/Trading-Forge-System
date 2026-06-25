using Microsoft.EntityFrameworkCore;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Repositories;
using TraderForge.Infrastructure.Persistence;

namespace TraderForge.Infrastructure.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly ApplicationDbContext _dbContext;

    public TransactionRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Transaction>> GetByPortfolioIdAsync(Guid portfolioId)
    {
        return await _dbContext.Transactions
            .Where(t => t.PortfolioId == portfolioId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }
}
