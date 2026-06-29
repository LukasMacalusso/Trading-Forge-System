using TraderForge.Domain.Enums;

namespace TraderForge.API.Requests;

public class AddBotNodeRequest
{
    public BotNodeType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Config { get; set; } = "{}";
    public double PositionX { get; set; }
    public double PositionY { get; set; }
}

public class UpdateBotNodeRequest
{
    public string Name { get; set; } = string.Empty;
    public string Config { get; set; } = "{}";
    public double PositionX { get; set; }
    public double PositionY { get; set; }
}

public class AddBotEdgeRequest
{
    public Guid SourceNodeId { get; set; }
    public NodePort SourcePort { get; set; }
    public Guid TargetNodeId { get; set; }
}
