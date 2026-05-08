using TraderForge.Domain.Entities;
namespace TraderForge.Domain.Repositories;

public interface IMarketAssetRepository
{
    Task AddAsync(MarketAsset price);
    Task SaveChangesAsync();
}