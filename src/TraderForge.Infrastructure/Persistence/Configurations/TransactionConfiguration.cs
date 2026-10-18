using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TraderForge.Domain.Entities;

namespace TraderForge.Infrastructure.Persistence.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();

        builder.Property(t => t.Type).IsRequired().HasMaxLength(20);
        builder.Property(t => t.Symbol).HasMaxLength(20);
        builder.Property(t => t.Quantity).HasColumnType("decimal(18,8)");
        builder.Property(t => t.Price).HasColumnType("decimal(18,2)");
        builder.Property(t => t.Commission).HasColumnType("decimal(18,2)");
        builder.Property(t => t.Total).HasColumnType("decimal(18,2)");
        builder.Property(t => t.BalanceBefore).HasColumnType("decimal(18,2)");
        builder.Property(t => t.BalanceAfter).HasColumnType("decimal(18,2)");

        builder.HasOne(t => t.Portfolio)
            .WithMany(p => p.Transactions)
            .HasForeignKey(t => t.PortfolioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
