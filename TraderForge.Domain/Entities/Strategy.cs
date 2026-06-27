namespace TraderForge.Domain.Entities;

public class Strategy
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public Guid PortfolioId { get; private set; }
    public Portfolio Portfolio { get; private set; } = null!;

    private Strategy() { }

    public Strategy(Guid id, string name, Guid portfolioId)
    {
        Id = id;
        Name = name;
        PortfolioId = portfolioId;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }
}
