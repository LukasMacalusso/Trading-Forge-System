using Microsoft.EntityFrameworkCore;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Interfaces;
using TraderForge.Infrastructure.Persistence;

namespace TraderForge.Infrastructure.Repositories;

public class PortfolioAssetRepository : IPortfolioAssetRepository
{
    private readonly ApplicationDbContext _dbContext;

    public PortfolioAssetRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<PortfolioAsset>> GetByTraderIdAsync(string traderId)
    {
        return await _dbContext.PortfolioAssets
            .Where(a => a.Portfolio.TraderId == traderId)
            .ToListAsync();
    }

    public async Task<PortfolioAsset?> GetByIdAsync(Guid id)
    {
        return await _dbContext.PortfolioAssets.FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task AddAsync(PortfolioAsset asset)
    {
        await _dbContext.PortfolioAssets.AddAsync(asset);
        await SaveChangesAsync();
    }

    public async Task RemoveAsync(PortfolioAsset asset)
    {
        _dbContext.PortfolioAssets.Remove(asset);
        await SaveChangesAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}
