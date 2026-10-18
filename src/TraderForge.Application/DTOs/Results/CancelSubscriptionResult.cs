using TraderForge.Domain.Services;

namespace TraderForge.Application.DTOs.Results;

public class CancelSubscriptionResult
{
    public bool WasCancelled { get; set; }
    public DiscountOffer? RetentionOffer { get; set; }
}
