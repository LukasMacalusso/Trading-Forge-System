using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TraderForge.Domain.Entities;

namespace TraderForge.Infrastructure.Persistence.Configurations;

public class StrategyExecutionConfiguration : IEntityTypeConfiguration<StrategyExecution>
{
    public void Configure(EntityTypeBuilder<StrategyExecution> builder)
    {
        builder.ToTable("StrategyExecutions");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.CurrentFlag).HasColumnType("jsonb");

        builder.HasOne(e => e.Strategy)
            .WithMany()
            .HasForeignKey(e => e.StrategyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
