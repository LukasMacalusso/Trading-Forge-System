namespace TraderForge.Domain.Services;

public record DiscountOffer(decimal Percentage, decimal DiscountedPrice);

public interface IDiscountService
{
    Task<DiscountOffer?> GetEarlyCancellationOfferAsync(string traderId, Guid targetPlanId);
}
