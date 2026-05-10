using TraderForge.Domain.Entities;

namespace TraderForge.Domain.Interfaces;

public interface ISubscriptionPlanRepository
{
    Task<SubscriptionPlan> GetByIdAsync(Guid id);

    Task<SubscriptionPlan> GetByNameAsync(string subscriptionName);
    
    Task<IEnumerable<SubscriptionPlan>> GetAllAsync();

    Task AddAsync(SubscriptionPlan newSubscriptionPlan);

    Task DeleteAsync(Guid id);

    Task SaveChangesAsync();
}