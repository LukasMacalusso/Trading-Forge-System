using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TraderForge.Domain.Entities;

namespace TraderForge.Infrastructure.Persistence.Configurations;

public class PositionConfiguration : IEntityTypeConfiguration<Position>
{
    public void Configure(EntityTypeBuilder<Position> builder)
    {
        builder.ToTable("Positions");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();
        builder.Property(a => a.Symbol).IsRequired().HasMaxLength(20);
        builder.Property(a => a.Quantity).HasColumnType("decimal(18,8)");
        builder.Property(a => a.EntryPrice).HasColumnType("decimal(18,2)");

        builder.HasOne(a => a.Portfolio)
            .WithMany(p => p.Positions)
            .HasForeignKey(a => a.PortfolioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
