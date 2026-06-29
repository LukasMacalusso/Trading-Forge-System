using Microsoft.EntityFrameworkCore;
using TraderForge.Domain.Entities;
using TraderForge.Infrastructure.Persistence;
using TraderForge.Infrastructure.Repositories;

namespace TraderForge.Integration.Tests;

public class StrategyRepositoryTests
{
    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task AddAsync_Then_GetByIdAsync_ReturnsStrategy()
    {
        using var ctx = CreateDbContext();
        var repo = new StrategyRepository(ctx);
        var portfolioId = Guid.NewGuid();
        var strategy = new Strategy(Guid.NewGuid(), "Test Strategy", portfolioId);

        await repo.AddAsync(strategy);

        var fetched = await repo.GetByIdAsync(strategy.Id);
        Assert.NotNull(fetched);
        Assert.Equal("Test Strategy", fetched.Name);
    }

    [Fact]
    public async Task Remove_Strategy_NoLongerQueryable()
    {
        using var ctx = CreateDbContext();
        var repo = new StrategyRepository(ctx);
        var portfolioId = Guid.NewGuid();
        var strategy = new Strategy(Guid.NewGuid(), "To Delete", portfolioId);
        await repo.AddAsync(strategy);

        repo.Remove(strategy);
        await repo.SaveChangesAsync();

        var fetched = await repo.GetByIdAsync(strategy.Id);
        Assert.Null(fetched);
    }

    [Fact]
    public async Task GetByPortfolioIdAsync_ReturnsOnlyMatching()
    {
        using var ctx = CreateDbContext();
        var repo = new StrategyRepository(ctx);
        var p1 = Guid.NewGuid();
        var p2 = Guid.NewGuid();
        await repo.AddAsync(new Strategy(Guid.NewGuid(), "S1", p1));
        await repo.AddAsync(new Strategy(Guid.NewGuid(), "S2", p1));
        await repo.AddAsync(new Strategy(Guid.NewGuid(), "S3", p2));

        var fromP1 = await repo.GetByPortfolioIdAsync(p1);
        Assert.Equal(2, fromP1.Count);
    }
}
