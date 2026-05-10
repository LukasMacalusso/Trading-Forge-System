using Microsoft.EntityFrameworkCore;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Interfaces;
using TraderForge.Infrastructure.Persistence;

namespace TraderForge.Infrastructure.Repositories;

public class AdministratorRepository : IAdministratorRepository
{
    private readonly ApplicationDbContext _dbContext;

    public AdministratorRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Administrator administrator)
    {
        await _dbContext.Administrators.AddAsync(administrator);
        await SaveChangesAsync();
    }

    public async Task<Administrator?> GetByIdAsync(string id)
    {
        return await _dbContext.Administrators.FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}