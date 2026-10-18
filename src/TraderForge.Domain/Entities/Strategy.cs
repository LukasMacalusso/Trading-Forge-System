namespace TraderForge.Domain.Entities;

public class Strategy
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public bool IsEngineActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Guid PortfolioId { get; private set; }
    public Portfolio Portfolio { get; private set; } = null!;

    public ICollection<BotNode> BotNodes { get; private set; } = new List<BotNode>();
    public ICollection<BotEdge> BotEdges { get; private set; } = new List<BotEdge>();

    private Strategy() { }

    public Strategy(Guid id, string name, Guid portfolioId)
    {
        Id = id;
        Name = name;
        PortfolioId = portfolioId;
        IsActive = true;
        IsEngineActive = false;
        CreatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        IsEngineActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void StartEngine() => IsEngineActive = true;
    public void StopEngine() => IsEngineActive = false;
}
