namespace TraderForge.API.Requests;

public class CreateStrategyRequest
{
    public string Name { get; set; } = string.Empty;
}

public class UpdateStrategyStateRequest
{
    public bool IsActive { get; set; }
}
