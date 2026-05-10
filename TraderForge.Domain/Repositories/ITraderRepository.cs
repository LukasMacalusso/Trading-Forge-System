using TraderForge.Domain.Entities;

namespace TraderForge.Domain.Repositories;

public interface ITraderRepository
{
    Task AddAsync(Trader trader);

    Task<Trader> GetByIdAsync(string id);
    
    Task<Trader> GetByIdIncludeSubPlanAsync(string id);
    
    Task<Trader> GetByIdIncludePortfolioAsync(string id);

    Task<Trader> GetByIdIncludeAllAsync(string id);

    Task<Trader> GetByIdIncludePlanAndStrategyAsync(string id);

    Task<Trader> GetByIdIncludePlanAndAssetsAsync(string id);

    Task<List<Trader>> GetExpiredTrialsAsync();
    
    Task SaveChangesAsync();
}