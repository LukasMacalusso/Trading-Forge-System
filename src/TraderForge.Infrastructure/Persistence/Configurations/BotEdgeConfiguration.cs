using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Enums;

namespace TraderForge.Infrastructure.Persistence.Configurations;

public class BotEdgeConfiguration : IEntityTypeConfiguration<BotEdge>
{
    public void Configure(EntityTypeBuilder<BotEdge> builder)
    {
        builder.ToTable("BotEdges");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.SourcePort)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(10)
            .HasDefaultValue(NodePort.Out);

        builder.HasOne(e => e.Strategy)
            .WithMany(s => s.BotEdges)
            .HasForeignKey(e => e.StrategyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.SourceNode)
            .WithMany(n => n.OutgoingEdges)
            .HasForeignKey(e => e.SourceNodeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.TargetNode)
            .WithMany(n => n.IncomingEdges)
            .HasForeignKey(e => e.TargetNodeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.SourceNodeId, e.SourcePort });
    }
}
