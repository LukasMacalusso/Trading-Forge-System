using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TraderForge.Domain.Entities;
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

    public async Task<Trader?> GetByIdAsync(string id)
    {
        return await _dbContext.Traders.FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Trader?> GetByIdIncludeSubPlanAsync(string id)
    {
        return await _dbContext.Traders.Include(t => t.Subscription!).ThenInclude(s => s.Plan).FirstOrDefaultAsync(t => t.Id == id);
    }
    
    public async Task<Trader?> GetByIdIncludePortfolioAsync(string id)
    {
        return await _dbContext.Traders.Include(t => t.Portfolios).FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Trader?> GetByIdIncludeAllAsync(string id)
    {
        return await _dbContext.Traders
            .Include(t => t.Subscription!).ThenInclude(s => s.Plan)
            .Include(t => t.Portfolios)
            .FirstOrDefaultAsync(t => t.Id == id);
    }
    
    public async Task<Trader?> GetByIdIncludePlanAndStrategyAsync(string id)
    {
        return await _dbContext.Traders
            .Include(t => t.Subscription!).ThenInclude(s => s.Plan)
            .Include(t => t.Portfolios)
            .ThenInclude(p => p.Strategies)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Trader?> GetByIdIncludePlanAndPositionsAsync(string id)
    {
        return await _dbContext.Traders
            .Include(t => t.Subscription!).ThenInclude(s => s.Plan)
            .Include(t => t.Portfolios)
            .ThenInclude(p => p.Positions)
            .FirstOrDefaultAsync(t => t.Id == id);
    }
    
    public async Task<IEnumerable<Trader>> GetAllIncludeSubPlanAsync()
    {
        return await _dbContext.Traders
            .Include(t => t.Subscription)
            .ToListAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}