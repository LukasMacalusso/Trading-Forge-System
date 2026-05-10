namespace TraderForge.Application.DTOs;

public class UpdateStrategyStateCommand
{
    public Guid StrategyId { get; set; }
    public bool IsActive { get; set; }
}
