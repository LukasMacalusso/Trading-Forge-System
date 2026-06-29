using TraderForge.Domain.Enums;

namespace TraderForge.Application.DTOs;

public class AddBotEdgeCommand
{
    public Guid StrategyId { get; set; }
    public Guid SourceNodeId { get; set; }
    public NodePort SourcePort { get; set; }
    public Guid TargetNodeId { get; set; }
}
