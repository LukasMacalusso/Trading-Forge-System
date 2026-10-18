using Microsoft.EntityFrameworkCore;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Repositories;
using TraderForge.Infrastructure.Persistence;

namespace TraderForge.Infrastructure.Repositories;

public class PendingOperationRepository : IPendingOperationRepository
{
    private readonly ApplicationDbContext _context;

    public PendingOperationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PendingOperation>> GetPendingByTraderIdAsync(string traderId)
    {
        return await _context.PendingOperations
            .Include(p => p.Portfolio)
            .Where(p => p.Portfolio.TraderId == traderId && !p.IsResolved)
            .OrderBy(p => p.ExpiresAt)
            .ToListAsync();
    }

    public async Task<PendingOperation?> GetByIdAsync(Guid id)
    {
        return await _context.PendingOperations
            .Include(p => p.Portfolio)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task AddAsync(PendingOperation pendingOperation)
    {
        await _context.PendingOperations.AddAsync(pendingOperation);
    }

    public void Update(PendingOperation pendingOperation)
    {
        _context.PendingOperations.Update(pendingOperation);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
