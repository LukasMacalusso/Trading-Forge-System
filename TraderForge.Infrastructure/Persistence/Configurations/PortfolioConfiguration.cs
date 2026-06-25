using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TraderForge.Domain.Entities;

namespace TraderForge.Infrastructure.Persistence.Configurations;

public class PortfolioConfiguration : IEntityTypeConfiguration<Portfolio>
{
    public void Configure(EntityTypeBuilder<Portfolio> builder)
    {
        builder.ToTable("Portfolios");
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.VirtualBalance).HasColumnType("decimal(18,2)");
        
        // -- Explicitly define the One-to-Many relationship (Trader -> Portfolios) -- //
        builder.HasOne(p => p.Trader)
            .WithMany(t => t.Portfolios)
            .HasForeignKey(p => p.TraderId)
            .OnDelete(DeleteBehavior.Cascade); // (UC - 11) If trader deleted, erase portfolios //
    }
}