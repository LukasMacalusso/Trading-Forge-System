using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TraderForge.Domain.Entities;

namespace TraderForge.Infrastructure.Persistence.Configurations;

public class BotNodeConfiguration : IEntityTypeConfiguration<BotNode>
{
    public void Configure(EntityTypeBuilder<BotNode> builder)
    {
        builder.ToTable("BotNodes");
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id).ValueGeneratedNever();
        builder.Property(n => n.Name).IsRequired().HasMaxLength(100);
        builder.Property(n => n.Type).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(n => n.Config).IsRequired().HasColumnType("jsonb");
        builder.Property(n => n.PositionX).HasDefaultValue(0);
        builder.Property(n => n.PositionY).HasDefaultValue(0);

        builder.HasOne(n => n.Strategy)
            .WithMany(s => s.BotNodes)
            .HasForeignKey(n => n.StrategyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(n => n.OutgoingEdges)
            .WithOne(e => e.SourceNode)
            .HasForeignKey(e => e.SourceNodeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(n => n.IncomingEdges)
            .WithOne(e => e.TargetNode)
            .HasForeignKey(e => e.TargetNodeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
