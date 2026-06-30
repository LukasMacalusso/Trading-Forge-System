using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TraderForge.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace TraderForge.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<Account>
{
    public DbSet<Trader> Traders { get; set; }
    public DbSet<Administrator> Administrators { get; set; }
    public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
    public DbSet<ActiveSubscription> ActiveSubscriptions { get; set; }
    public DbSet<Portfolio> Portfolios { get; set; }

    public DbSet<Strategy> Strategies { get; set; }
    public DbSet<Position> Positions { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Order> Orders { get; set; }

    public DbSet<BotNode> BotNodes { get; set; }
    public DbSet<BotEdge> BotEdges { get; set; }
    public DbSet<StrategyExecution> StrategyExecutions { get; set; }

    public DbSet<PendingOperation> PendingOperations { get; set; }
    
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)

    { // empty because inheriting the base constructor
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
