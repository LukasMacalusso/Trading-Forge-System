using TraderForge.Domain.Enums;

namespace TraderForge.Domain.Entities;

public class StrategyExecution
{
    public Guid Id { get; private set; }
    public Guid StrategyId { get; private set; }
    public Strategy Strategy { get; private set; } = null!;
    public ExecutionStatus Status { get; private set; }
    public string? CurrentFlag { get; private set; }
    public Guid? CurrentNodeId { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? LastActivityAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private StrategyExecution() { }

    public StrategyExecution(Guid strategyId)
    {
        Id = Guid.NewGuid();
        StrategyId = strategyId;
        Status = ExecutionStatus.Running;
        StartedAt = DateTime.UtcNow;
        LastActivityAt = DateTime.UtcNow;
    }

    public void Pause()
    {
        Status = ExecutionStatus.Paused;
        LastActivityAt = DateTime.UtcNow;
    }

    public void Complete()
    {
        Status = ExecutionStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }

    public void Fail()
    {
        Status = ExecutionStatus.Failed;
        CompletedAt = DateTime.UtcNow;
    }

    public void Expire()
    {
        Status = ExecutionStatus.Expired;
        CompletedAt = DateTime.UtcNow;
    }

    public void UpdateProgress(Guid? currentNodeId, string? flag)
    {
        CurrentNodeId = currentNodeId;
        CurrentFlag = flag;
        LastActivityAt = DateTime.UtcNow;
    }
}
