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
    public DbSet<Portfolio> Portfolios { get; set; }
    public DbSet<MarketAsset> MarketAssets { get; set; }
    public DbSet<Strategy> Strategies { get; set; }
    public DbSet<PortfolioAsset> PortfolioAssets { get; set; }
    
    
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    { // empty because inheriting the base constructor
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
