using System;

namespace TraderForge.Domain.Entities;

public class PendingOperation
{
    public Guid Id { get; private set; }
    public Guid PortfolioId { get; private set; }
    public Portfolio Portfolio { get; private set; } = null!;
    
    public Guid StrategyId { get; private set; }
    public string StrategyName { get; private set; } = string.Empty;
    
    public string Symbol { get; private set; } = string.Empty;
    public string Action { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public decimal CurrentPrice { get; private set; }
    
    public DateTime ConditionMetAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    
    public bool IsResolved { get; private set; }

    private PendingOperation() { }

    public PendingOperation(Guid id, Guid portfolioId, Guid strategyId, string strategyName, 
        string symbol, string action, decimal quantity, decimal currentPrice, DateTime expiresAt)
    {
        Id = id;
        PortfolioId = portfolioId;
        StrategyId = strategyId;
        StrategyName = strategyName;
        Symbol = symbol;
        Action = action;
        Quantity = quantity;
        CurrentPrice = currentPrice;
        ConditionMetAt = DateTime.UtcNow;
        ExpiresAt = expiresAt;
        IsResolved = false;
    }

    public void Resolve()
    {
        IsResolved = true;
    }
}
