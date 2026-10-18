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
        var portfolio = new Portfolio("trader1", 1000m);
        ctx.Portfolios.Add(portfolio);
        await ctx.SaveChangesAsync();

        var repo = new StrategyRepository(ctx);
        var strategy = new Strategy(Guid.NewGuid(), "Test Strategy", portfolio.Id);

        await repo.AddAsync(strategy);

        var fetched = await repo.GetByIdAsync(strategy.Id);
        Assert.NotNull(fetched);
        Assert.Equal("Test Strategy", fetched.Name);
    }

    [Fact]
    public async Task Remove_Strategy_NoLongerQueryable()
    {
        using var ctx = CreateDbContext();
        var portfolio = new Portfolio("trader1", 1000m);
        ctx.Portfolios.Add(portfolio);
        await ctx.SaveChangesAsync();

        var repo = new StrategyRepository(ctx);
        var strategy = new Strategy(Guid.NewGuid(), "To Delete", portfolio.Id);
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
        var p1 = new Portfolio("trader1", 1000m);
        var p2 = new Portfolio("trader2", 1000m);
        ctx.Portfolios.Add(p1);
        ctx.Portfolios.Add(p2);
        await ctx.SaveChangesAsync();

        var repo = new StrategyRepository(ctx);
        await repo.AddAsync(new Strategy(Guid.NewGuid(), "S1", p1.Id));
        await repo.AddAsync(new Strategy(Guid.NewGuid(), "S2", p1.Id));
        await repo.AddAsync(new Strategy(Guid.NewGuid(), "S3", p2.Id));

        var fromP1 = await repo.GetByPortfolioIdAsync(p1.Id);
        Assert.Equal(2, fromP1.Count);
    }

    [Fact]
    public async Task GetByIdWithGraphAsync_IncludesBotNodesAndEdges()
    {
        using var ctx = CreateDbContext();
        var portfolio = new Portfolio("trader1", 1000m);
        ctx.Portfolios.Add(portfolio);
        await ctx.SaveChangesAsync();

        var repo = new StrategyRepository(ctx);
        var strategy = new Strategy(Guid.NewGuid(), "Graph Strategy", portfolio.Id);
        var node = new BotNode(strategy.Id, Domain.Enums.BotNodeType.Trigger, "T1", "{}", 0, 0);
        strategy.BotNodes.Add(node);
        await repo.AddAsync(strategy);

        var fetched = await repo.GetByIdWithGraphAsync(strategy.Id);
        Assert.NotNull(fetched);
        Assert.Single(fetched.BotNodes);
    }

    [Fact]
    public async Task GetByIdWithGraphAsync_NotFound_ReturnsNull()
    {
        using var ctx = CreateDbContext();
        var repo = new StrategyRepository(ctx);
        var result = await repo.GetByIdWithGraphAsync(Guid.NewGuid());
        Assert.Null(result);
    }

    [Fact]
    public async Task GetActiveWithEngineRunningAsync_ReturnsOnlyActiveEngines()
    {
        using var ctx = CreateDbContext();
        var portfolio = new Portfolio("trader1", 1000m);
        ctx.Portfolios.Add(portfolio);
        await ctx.SaveChangesAsync();

        var repo = new StrategyRepository(ctx);
        var activeStrategy = new Strategy(Guid.NewGuid(), "Active", portfolio.Id);
        activeStrategy.StartEngine();
        await repo.AddAsync(activeStrategy);

        var inactiveStrategy = new Strategy(Guid.NewGuid(), "Inactive", portfolio.Id);
        await repo.AddAsync(inactiveStrategy);

        var result = await repo.GetActiveWithEngineRunningAsync();
        Assert.Single(result);
        Assert.Equal("Active", result[0].Name);
    }
}
