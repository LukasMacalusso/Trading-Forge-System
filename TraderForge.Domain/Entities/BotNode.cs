using TraderForge.Domain.Enums;

namespace TraderForge.Domain.Entities;

public class BotNode
{
    public Guid Id { get; private set; }
    public Guid StrategyId { get; private set; }
    public Strategy Strategy { get; private set; } = null!;
    public BotNodeType Type { get; private set; }
    public string Name { get; private set; } = null!;
    public string Config { get; private set; } = null!;
    public double PositionX { get; private set; }
    public double PositionY { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public ICollection<BotEdge> OutgoingEdges { get; private set; } = new List<BotEdge>();
    public ICollection<BotEdge> IncomingEdges { get; private set; } = new List<BotEdge>();

    private BotNode() { }

    public BotNode(Guid strategyId, BotNodeType type, string name, string config, double posX, double posY)
    {
        Id = Guid.NewGuid();
        StrategyId = strategyId;
        Type = type;
        Name = name;
        Config = config;
        PositionX = posX;
        PositionY = posY;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(string name, string config, double posX, double posY)
    {
        Name = name;
        Config = config;
        PositionX = posX;
        PositionY = posY;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
