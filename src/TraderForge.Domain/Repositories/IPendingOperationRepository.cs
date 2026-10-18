using TraderForge.Domain.Entities;

namespace TraderForge.Domain.Repositories;

public interface IPendingOperationRepository
{
    Task<List<PendingOperation>> GetPendingByTraderIdAsync(string traderId);
    Task<PendingOperation?> GetByIdAsync(Guid id);
    Task AddAsync(PendingOperation pendingOperation);
    void Update(PendingOperation pendingOperation);
    Task SaveChangesAsync();
}
