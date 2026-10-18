using TraderForge.Domain.Enums;

namespace TraderForge.Domain.Entities;

public class BotEdge
{
    public Guid Id { get; private set; }
    public Guid StrategyId { get; private set; }
    public Strategy Strategy { get; private set; } = null!;
    public Guid SourceNodeId { get; private set; }
    public BotNode SourceNode { get; private set; } = null!;
    public NodePort SourcePort { get; private set; }
    public Guid TargetNodeId { get; private set; }
    public BotNode TargetNode { get; private set; } = null!;

    private BotEdge() { }

    public BotEdge(Guid strategyId, Guid sourceNodeId, NodePort sourcePort, Guid targetNodeId)
    {
        Id = Guid.NewGuid();
        StrategyId = strategyId;
        SourceNodeId = sourceNodeId;
        SourcePort = sourcePort;
        TargetNodeId = targetNodeId;
    }
}
