using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TraderForge.Domain.Entities;

namespace TraderForge.Infrastructure.Persistence.Configurations;

public class ActiveSubscriptionConfiguration : IEntityTypeConfiguration<ActiveSubscription>
{
    public void Configure(EntityTypeBuilder<ActiveSubscription> builder)
    {
        builder.HasKey(s => s.Id);

        builder.HasOne(s => s.Trader)
               .WithOne(t => t.Subscription)
               .HasForeignKey<ActiveSubscription>(s => s.TraderId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Plan)
               .WithMany()
               .HasForeignKey(s => s.SubscriptionPlanId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
