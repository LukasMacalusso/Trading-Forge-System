namespace TraderForge.API.Requests;

public class ChangeSubscriptionRequest
{
    public Guid NewPlanId { get; set; }
    public string? PromoCode { get; set; }
}
