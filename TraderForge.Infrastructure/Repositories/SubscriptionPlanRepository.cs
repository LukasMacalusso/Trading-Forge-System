using Microsoft.EntityFrameworkCore;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Repositories;
using TraderForge.Infrastructure.Persistence;

namespace TraderForge.Infrastructure.Repositories;

public class SubscriptionPlanRepository : ISubscriptionPlanRepository
{
    private readonly ApplicationDbContext _dbContext;

    public SubscriptionPlanRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SubscriptionPlan?> GetByIdAsync(Guid id)
    {
        return await _dbContext.SubscriptionPlans.FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<IEnumerable<SubscriptionPlan>> GetAllAsync()
    {
        return await _dbContext.SubscriptionPlans.ToListAsync();
    }

    public async Task<SubscriptionPlan?> GetByNameAsync(string subscriptionName)
    {
        return await _dbContext.SubscriptionPlans.FirstOrDefaultAsync(s => s.Name.ToLower() == subscriptionName.ToLower());
    }

    public async Task AddAsync(SubscriptionPlan newSubscriptionPlan)
    {
        await _dbContext.SubscriptionPlans.AddAsync(newSubscriptionPlan);
        await SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var deletingPlan = await GetByIdAsync(id);
        if (deletingPlan != null) _dbContext.Remove(deletingPlan);
        await SaveChangesAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}
