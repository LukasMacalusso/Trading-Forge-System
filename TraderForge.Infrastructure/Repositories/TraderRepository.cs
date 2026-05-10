using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Interfaces;
using TraderForge.Infrastructure.Persistence;
using System.Linq;
using TraderForge.Domain.Repositories;

namespace TraderForge.Infrastructure.Repositories;

public class TraderRepository : ITraderRepository
{
    private readonly ApplicationDbContext _dbContext;

    public TraderRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Trader trader)
    {
        await _dbContext.Traders.AddAsync(trader);
        await SaveChangesAsync();
    }

    public async Task<Trader> GetByIdAsync(string id)
    {
        return await _dbContext.Traders.FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Trader> GetByIdIncludeSubPlanAsync(string id)
    {
        return await _dbContext.Traders.Include(t => t.SubscriptionPlan).FirstOrDefaultAsync(t => t.Id == id);
    }
    
    public async Task<Trader> GetByIdIncludePortfolioAsync(string id)
    {
        return await _dbContext.Traders.Include(t => t.Portfolios).FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Trader> GetByIdIncludeAllAsync(string id)
    {
        return (await _dbContext.Traders
            .Include(t => t.SubscriptionPlan)
            .Include(t => t.Portfolios)
            .FirstOrDefaultAsync(t => t.Id == id))!;
    }
    
    public async Task<Trader> GetByIdIncludePlanAndStrategyAsync(string id)
    {
        return (await _dbContext.Traders
            .Include(t => t.SubscriptionPlan)
            .Include(t => t.Portfolios)
            .ThenInclude(p => p.Strategies)
            .FirstOrDefaultAsync(t => t.Id == id))!;
    }

    public async Task<Trader> GetByIdIncludePlanAndAssetsAsync(string id)
    {
        return (await _dbContext.Traders
            .Include(t => t.SubscriptionPlan)
            .Include(t => t.Portfolios)
            .ThenInclude(p => p.PortfolioAssets)
            .FirstOrDefaultAsync(t => t.Id == id))!;
    }

    public async Task<List<Trader>> GetExpiredTrialsAsync()
    {
        return await _dbContext.Traders
            .Where(t => t.FreeTrialExpirationDate < DateTime.UtcNow && t.SubscriptionPlanId != null).ToListAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}