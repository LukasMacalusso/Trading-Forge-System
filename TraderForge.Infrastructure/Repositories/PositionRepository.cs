using Microsoft.EntityFrameworkCore;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Repositories;
using TraderForge.Infrastructure.Persistence;

namespace TraderForge.Infrastructure.Repositories;

public class PositionRepository : IPositionRepository
{
    private readonly ApplicationDbContext _dbContext;

    public PositionRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Position>> GetByPortfolioIdAsync(Guid portfolioId)
    {
        return await _dbContext.Positions
            .Where(a => a.PortfolioId == portfolioId)
            .ToListAsync();
    }

    public async Task<Position?> GetByIdAsync(Guid id)
    {
        return await _dbContext.Positions.FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Position?> GetByIdWithPortfolioAsync(Guid id)
    {
        return await _dbContext.Positions
            .Include(p => p.Portfolio)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task AddAsync(Position asset)
    {
        await _dbContext.Positions.AddAsync(asset);
        await SaveChangesAsync();
    }

    public async Task RemoveAsync(Position asset)
    {
        _dbContext.Positions.Remove(asset);
        await SaveChangesAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}
