using TraderForge.API.Requests;
using TraderForge.Application.DTOs;

namespace TraderForge.API.Mappers;

public static class SubscriptionMappers
{
    public static ChangeSubscriptionCommand ToCommand(this ChangeSubscriptionRequest request, string traderId)
    {
        return new ChangeSubscriptionCommand
        {
            TraderId = traderId,
            NewPlanId = request.NewPlanId,
            PromoCode = request.PromoCode
        };
    }
}
