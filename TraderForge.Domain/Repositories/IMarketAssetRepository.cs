using TraderForge.Domain.Entities;

namespace TraderForge.Domain.Interfaces;

public interface IMarketAssetRepository
{
    Task AddAsync(MarketAsset asset);
    Task SaveChangesAsync();
}
