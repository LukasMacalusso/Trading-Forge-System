using TraderForge.Domain.Entities;

namespace TraderForge.Domain.Repositories;

public interface IAdministratorRepository
{
    Task AddAsync(Administrator administrator);
    Task<Administrator?> GetByIdAsync(string id);
    Task SaveChangesAsync();
}
