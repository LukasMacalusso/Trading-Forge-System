using TraderForge.Domain.Common;
using TraderForge.Application.DTOs;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Repositories;

namespace TraderForge.Application.Handlers;

public class GetTraderPlanQueryHandler
{
    private readonly ITraderRepository _traderRepository;
    public GetTraderPlanQueryHandler(ITraderRepository traderRepository)
    {
        _traderRepository = traderRepository;
    }
    public async Task<ResultGeneric<SubscriptionPlan?>> HandleAsync(GetTraderPlanQuery query)
    {
        try
        {
            var trader = await _traderRepository.GetByIdIncludeSubPlanAsync(query.TraderId);
            
            if (trader is null)
                return ResultGeneric<SubscriptionPlan?>.Failure("Trader not found.");
            return ResultGeneric<SubscriptionPlan?>.Success(trader.SubscriptionPlan);
            
        }
        catch (Exception ex)
        {
            return ResultGeneric<SubscriptionPlan?>.Failure(ex.Message);
        }
    }
}