namespace TraderForge.Application.DTOs.Responses;

public class TraderSummaryResponse
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsSuspended { get; set; }
    public string? SuspensionReason { get; set; }
    public string? ActivePlanId { get; set; }
}
