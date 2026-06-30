using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TraderForge.Domain.Entities;

namespace TraderForge.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).ValueGeneratedNever();

        builder.Property(o => o.Symbol)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(o => o.Side)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(o => o.Type)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(o => o.Status)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(o => o.Quantity)
            .HasColumnType("decimal(18,8)");

        builder.Property(o => o.Price)
            .HasColumnType("decimal(18,8)");

        builder.Property(o => o.Commission)
            .HasColumnType("decimal(18,2)");

        builder.Property(o => o.Total)
            .HasColumnType("decimal(18,2)");

        builder.HasOne(o => o.Portfolio)
            .WithMany(p => p.Orders)
            .HasForeignKey(o => o.PortfolioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
