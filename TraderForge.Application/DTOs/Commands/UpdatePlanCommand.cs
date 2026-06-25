namespace TraderForge.Application.DTOs;

public class UpdatePlanCommand
{
    public Guid PlanId { get; set; }
    public string Name { get; set; }
    public decimal MonthlyPrice { get; set; }
    public decimal InitialVirtualBalance { get; set; }
    public int? MaxActiveStrategies { get; set; }
    public int? MaxActiveAssets { get; set; }
    public bool CanModifyVirtualBalance { get; set; }
}
