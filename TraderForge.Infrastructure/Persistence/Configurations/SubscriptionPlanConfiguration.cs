using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TraderForge.Domain.Entities;

namespace TraderForge.Infrastructure.Persistence.Configurations;

public class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
    {
        builder.ToTable("SubscriptionPlans");
        builder.HasKey(p => p.Id);

        // - Property Constraints - //
        builder.Property(p => p.Name).IsRequired().HasMaxLength(50);
        builder.Property(p => p.MonthlyPrice).HasColumnType("decimal(18,2)");
        builder.Property(p => p.InitialVirtualBalance).HasColumnType("decimal(18,2)");
        
        // -- TIER DEFINITION -- //
        builder.HasData(
            new SubscriptionPlan(
                id: Guid.Parse("11111111-1111-1111-1111-111111111111"),
                name: "Basic",
                monthlyPrice: 9.99m,
                initialVirtualBalance: 10000m,
                maxActiveStrategies: 2,
                maxActiveAssets: 5,
                canModifyVirtualBalance: false
            ),
            new SubscriptionPlan(
                id: Guid.Parse("22222222-2222-2222-2222-222222222222"),
                name: "Pro",
                monthlyPrice: 29.99m,
                initialVirtualBalance: 50000m,
                maxActiveStrategies: 10,
                maxActiveAssets: 20,
                canModifyVirtualBalance: false
            ),
            new SubscriptionPlan(
                id: Guid.Parse("33333333-3333-3333-3333-333333333333"),
                name: "Enterprise",
                monthlyPrice: 99.99m,
                initialVirtualBalance: 100000m,
                maxActiveStrategies: null, // Unlimited (BR-5)
                maxActiveAssets: null,     // Unlimited (BR-5)
                canModifyVirtualBalance: true // (BR-5)
            )
        );       
    }
}