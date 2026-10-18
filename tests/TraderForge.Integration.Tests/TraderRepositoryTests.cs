using Microsoft.EntityFrameworkCore;
using TraderForge.Domain.Entities;
using TraderForge.Infrastructure.Persistence;
using TraderForge.Infrastructure.Repositories;

namespace TraderForge.Integration.Tests;

public class TraderRepositoryTests
{
    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task AddAsync_Then_GetByIdAsync_ReturnsTrader()
    {
        using var ctx = CreateDbContext();
        var repo = new TraderRepository(ctx);
        var traderId = Guid.NewGuid().ToString();
        var trader = new Trader(traderId, "test@test.com");

        await repo.AddAsync(trader);

        var fetched = await repo.GetByIdAsync(traderId);
        Assert.NotNull(fetched);
        Assert.Equal("test@test.com", fetched.Email);
    }

    [Fact]
    public async Task GetByIdIncludePortfolioAsync_IncludesPortfolios()
    {
        using var ctx = CreateDbContext();
        var repo = new TraderRepository(ctx);
        var traderId = Guid.NewGuid().ToString();
        var trader = new Trader(traderId, "test@test.com");
        trader.Portfolios.Add(new Portfolio(traderId, 10000m));

        await repo.AddAsync(trader);

        var fetched = await repo.GetByIdIncludePortfolioAsync(traderId);
        Assert.NotNull(fetched);
        Assert.Single(fetched.Portfolios);
    }

    [Fact]
    public async Task GetByIdIncludePortfolioAsync_NotFound_ReturnsNull()
    {
        using var ctx = CreateDbContext();
        var repo = new TraderRepository(ctx);

        var fetched = await repo.GetByIdIncludePortfolioAsync("nonexistent");

        Assert.Null(fetched);
    }
}
