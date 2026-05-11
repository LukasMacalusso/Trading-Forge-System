using TraderForge.Domain.Entities;

namespace TraderForge.Domain.Repositories;

public interface IPortfolioAssetRepository
{
    Task<PortfolioAsset?> GetByIdAsync(Guid id);
    Task<List<PortfolioAsset>> GetByTraderIdAsync(string traderId);
    Task AddAsync(PortfolioAsset asset);
    Task RemoveAsync(PortfolioAsset asset);
    Task SaveChangesAsync();
}
