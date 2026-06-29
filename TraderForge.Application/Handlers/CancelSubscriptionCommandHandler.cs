using MediatR;
using TraderForge.Application.DTOs;
using TraderForge.Application.DTOs.Results;
using TraderForge.Application.Events;
using TraderForge.Domain.Common;
using TraderForge.Domain.Repositories;
using TraderForge.Domain.Services;

namespace TraderForge.Application.Handlers;

public class CancelSubscriptionCommandHandler
{
    private readonly ITraderRepository _traderRepository;
    private readonly IDiscountService _discountService;
    private readonly ISubscriptionPlanRepository _planRepository;
    private readonly IPublisher _publisher;

    public CancelSubscriptionCommandHandler(
        ITraderRepository traderRepository,
        IDiscountService discountService,
        ISubscriptionPlanRepository planRepository,
        IPublisher publisher)
    {
        _traderRepository = traderRepository;
        _discountService = discountService;
        _planRepository = planRepository;
        _publisher = publisher;
    }

    public async Task<ResultGeneric<CancelSubscriptionResult>> HandleAsync(CancelSubscriptionCommand command)
    {
        try
        {
            return await ExecuteSubscriptionCancel(command);
        }
        catch (Exception ex)
        {
            return ResultGeneric<CancelSubscriptionResult>.Failure(ex.Message);
        }
    }

    private async Task<ResultGeneric<CancelSubscriptionResult>> ExecuteSubscriptionCancel(CancelSubscriptionCommand command) 
    {
        var trader = await _traderRepository.GetByIdIncludeAllAsync(command.TraderId);
        if (trader == null) return ResultGeneric<CancelSubscriptionResult>.Failure("Trader not found.");
        
        var allPlans = await _planRepository.GetAllAsync();
        var premiumPlan = allPlans.FirstOrDefault(p => p.Name.ToLower() != "basic");

        DiscountOffer? discountOffer = null;
        if (premiumPlan != null)
        {
            discountOffer = await _discountService.GetEarlyCancellationOfferAsync(trader.Id, premiumPlan.Id);
        }
        
        if (discountOffer != null && !command.ForceCancel)
        {
            // Do not cancel yet. Present the retention offer.
            return ResultGeneric<CancelSubscriptionResult>.Success(new CancelSubscriptionResult 
            { 
                WasCancelled = false, 
                RetentionOffer = discountOffer 
            });
        }
        
        trader.CancelSubscription();
        await _traderRepository.SaveChangesAsync();
        
        await _publisher.Publish(new SubscriptionCancelledEvent(trader.Email, trader.UserName));
        
        return ResultGeneric<CancelSubscriptionResult>.Success(new CancelSubscriptionResult 
        { 
            WasCancelled = true, 
            RetentionOffer = discountOffer 
        });
    }
}

