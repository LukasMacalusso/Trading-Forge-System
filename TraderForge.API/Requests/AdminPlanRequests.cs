namespace TraderForge.API.Requests;

public class CreatePlanRequest
{
    public string Name { get; set; }
    public decimal MonthlyPrice { get; set; }
    public decimal InitialVirtualBalance { get; set; }
    public int? MaxActiveStrategies { get; set; }
    public int? MaxActiveAssets { get; set; }
    public bool CanModifyVirtualBalance { get; set; }
}

public class UpdatePlanRequest
{
    public string Name { get; set; }
    public decimal MonthlyPrice { get; set; }
    public decimal InitialVirtualBalance { get; set; }
    public int? MaxActiveStrategies { get; set; }
    public int? MaxActiveAssets { get; set; }
    public bool CanModifyVirtualBalance { get; set; }
}
