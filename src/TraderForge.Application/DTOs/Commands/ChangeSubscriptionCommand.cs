namespace TraderForge.Application.DTOs;

public class ChangeSubscriptionCommand
{
    public string TraderId { get; set; } = string.Empty;
    public Guid NewPlanId { get; set; }
    public string? PromoCode { get; set; }

}
