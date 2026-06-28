using TraderForge.Application.DTOs;
using TraderForge.Domain.Common;
using TraderForge.Domain.Repositories;
using TraderForge.Domain.Services;

namespace TraderForge.Application.Handlers;

public class CancelSubscriptionCommandHandler
{
    private readonly ITraderRepository _traderRepository;
    private readonly ISubscriptionPlanRepository _subscriptionPlanRepository;
    private readonly ISubscriptionLimitGuard _limitGuard;

    public CancelSubscriptionCommandHandler(
        ITraderRepository traderRepository,
        ISubscriptionPlanRepository subscriptionPlanRepository,
        ISubscriptionLimitGuard limitGuard)
    {
        _traderRepository = traderRepository;
        _subscriptionPlanRepository = subscriptionPlanRepository;
        _limitGuard = limitGuard;
    }

    public async Task<Result> HandleAsync(ChangeSubscriptionCommand command)
    {
        try
        {
            return await ExecuteSubscriptionCancel(command);
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }
    }

    private async Task<Result> ExecuteSubscriptionCancel(ChangeSubscriptionCommand command) 
    {
        /*
        var trader = await _traderRepository.GetByIdIncludeAllAsync(command.TraderId);
        if (trader == null) 
        {
            return Result.Failure("Trader not found.");
        }

        var newSubscriptionPlan = await _subscriptionPlanRepository.GetByIdAsync(command.NewPlanId);
        if (newSubscriptionPlan == null) 
        {
            return Result.Failure("Subscription Plan not found.");
        }

        var canSwitch = await _limitGuard.CanSwitchToPlanAsync(trader.Id, newSubscriptionPlan);
        if (!canSwitch)
        {
            return Result.Failure("Cannot switch plan: current active strategies or assets exceed the new plan limits.");
        }

        trader.ProcessPayment(newSubscriptionPlan);
        
        await _traderRepository.SaveChangesAsync();
            */
        return Result.Success();
    }
}