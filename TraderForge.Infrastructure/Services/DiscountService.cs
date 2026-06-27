using Microsoft.Extensions.Configuration;
using TraderForge.Domain.Services;
using TraderForge.Domain.Repositories;

namespace TraderForge.Infrastructure.Services;

public class DiscountService : IDiscountService
{
    private readonly ITraderRepository _traderRepository;
    private readonly ISubscriptionPlanRepository _planRepository;
    private readonly IConfiguration _configuration;

    public DiscountService(
        ITraderRepository traderRepository,
        ISubscriptionPlanRepository planRepository,
        IConfiguration configuration)
    {
        _traderRepository = traderRepository;
        _planRepository = planRepository;
        _configuration = configuration;
    }

    public async Task<DiscountOffer?> GetEarlyCancellationOfferAsync(string traderId, Guid targetPlanId)
    {
        var trader = await _traderRepository.GetByIdIncludeSubPlanAsync(traderId);
        var plan = await _planRepository.GetByIdAsync(targetPlanId);

        if (trader is null || plan is null)
            return null;

        bool isOnTrial = trader.Subscription != null && 
                         trader.Subscription.IsActive && 
                         trader.Subscription.Plan.Name.ToLower() == "basic";

        if (!isOnTrial)
            return null;

        var discountPct = _configuration.GetValue<decimal>(
            "SubscriptionSettings:EarlyCancellationDiscountPercentage");

        var discountedPrice = plan.MonthlyPrice * (1 - discountPct / 100);

        return new DiscountOffer(discountPct, discountedPrice);
    }
}
