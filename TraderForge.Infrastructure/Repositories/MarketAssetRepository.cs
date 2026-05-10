using TraderForge.Domain.Entities;
using TraderForge.Domain.Repositories;
using TraderForge.Infrastructure.Persistence;

namespace TraderForge.Domain.Interfaces;

public class MarketAssetRepository : IMarketAssetRepository
{
    private readonly ApplicationDbContext _dbContext;

    public MarketAssetRepository(ApplicationDbContext dbContext)
        => _dbContext = dbContext;

    public async Task AddAsync(MarketAsset asset)
    {
        await _dbContext.MarketAssets.AddAsync(asset);
    }

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}
