using Microsoft.EntityFrameworkCore;
using TraderForge.Domain.Entities;
using TraderForge.Infrastructure.Persistence.Configurations;

namespace TraderForge.Infrastructure.Tests;

public class EntityConfigurationTests
{
    [Fact]
    public void BotNodeConfiguration_Apply_SetsTableName()
    {
        var builder = new ModelBuilder();
        builder.ApplyConfiguration(new BotNodeConfiguration());
        var entity = builder.Model.FindEntityType(typeof(BotNode));
        Assert.NotNull(entity);
        Assert.Equal("BotNodes", entity.GetTableName());
    }

    [Fact]
    public void BotEdgeConfiguration_Apply_SetsTableName()
    {
        var builder = new ModelBuilder();
        builder.ApplyConfiguration(new BotEdgeConfiguration());
        var entity = builder.Model.FindEntityType(typeof(BotEdge));
        Assert.NotNull(entity);
        Assert.Equal("BotEdges", entity.GetTableName());
    }

    [Fact]
    public void StrategyExecutionConfiguration_Apply_SetsTableName()
    {
        var builder = new ModelBuilder();
        builder.ApplyConfiguration(new StrategyExecutionConfiguration());
        var entity = builder.Model.FindEntityType(typeof(StrategyExecution));
        Assert.NotNull(entity);
        Assert.Equal("StrategyExecutions", entity.GetTableName());
    }

    [Fact]
    public void StrategyConfiguration_Apply_SetsTableName()
    {
        var builder = new ModelBuilder();
        builder.ApplyConfiguration(new StrategyConfiguration());
        var entity = builder.Model.FindEntityType(typeof(Strategy));
        Assert.NotNull(entity);
        Assert.Equal("Strategies", entity.GetTableName());
    }
}
