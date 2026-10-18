using Microsoft.EntityFrameworkCore;
using TraderForge.Domain.Entities;
using TraderForge.Infrastructure.Persistence;
using TraderForge.Infrastructure.Repositories;

namespace TraderForge.Integration.Tests;

public class PendingOperationRepositoryTests
{
    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task AddAsync_Then_GetByIdAsync_ReturnsOperation()
    {
        using var ctx = CreateDbContext();
        var trader = new Trader("trader1", "test@test.com");
        var portfolio = new Portfolio("trader1", 10000m);
        ctx.Traders.Add(trader);
        ctx.Portfolios.Add(portfolio);
        await ctx.SaveChangesAsync();

        var repo = new PendingOperationRepository(ctx);
        var op = new PendingOperation(Guid.NewGuid(), portfolio.Id, Guid.NewGuid(), "Strat", "BTC", "buy", 1m, 50000m, DateTime.UtcNow.AddMinutes(5));
        await repo.AddAsync(op);
        await repo.SaveChangesAsync();

        var fetched = await repo.GetByIdAsync(op.Id);
        Assert.NotNull(fetched);
        Assert.Equal("BTC", fetched.Symbol);
    }

    [Fact]
    public async Task GetPendingByTraderIdAsync_ReturnsOnlyUnresolved()
    {
        using var ctx = CreateDbContext();
        var trader = new Trader("trader1", "test@test.com");
        var portfolio = new Portfolio("trader1", 10000m);
        ctx.Traders.Add(trader);
        ctx.Portfolios.Add(portfolio);
        await ctx.SaveChangesAsync();

        var repo = new PendingOperationRepository(ctx);
        var op1 = new PendingOperation(Guid.NewGuid(), portfolio.Id, Guid.NewGuid(), "Strat", "BTC", "buy", 1m, 50000m, DateTime.UtcNow.AddMinutes(5));
        var op2 = new PendingOperation(Guid.NewGuid(), portfolio.Id, Guid.NewGuid(), "Strat", "ETH", "sell", 2m, 3000m, DateTime.UtcNow.AddMinutes(5));
        op2.Resolve();
        await repo.AddAsync(op1);
        await repo.AddAsync(op2);
        await repo.SaveChangesAsync();

        var pending = await repo.GetPendingByTraderIdAsync("trader1");
        Assert.Single(pending);
        Assert.Equal("BTC", pending[0].Symbol);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        using var ctx = CreateDbContext();
        var repo = new PendingOperationRepository(ctx);

        var fetched = await repo.GetByIdAsync(Guid.NewGuid());
        Assert.Null(fetched);
    }

    [Fact]
    public async Task Update_MarksAsResolved()
    {
        using var ctx = CreateDbContext();
        var portfolio = new Portfolio("trader1", 10000m);
        ctx.Portfolios.Add(portfolio);
        await ctx.SaveChangesAsync();

        var repo = new PendingOperationRepository(ctx);
        var op = new PendingOperation(Guid.NewGuid(), portfolio.Id, Guid.NewGuid(), "Strat", "BTC", "buy", 1m, 50000m, DateTime.UtcNow.AddMinutes(5));
        await repo.AddAsync(op);
        await repo.SaveChangesAsync();

        op.Resolve();
        repo.Update(op);
        await repo.SaveChangesAsync();

        var fetched = await repo.GetByIdAsync(op.Id);
        Assert.NotNull(fetched);
        Assert.True(fetched.IsResolved);
    }
}
