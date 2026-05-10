using TraderForge.API.Requests;
using TraderForge.Application.DTOs;

namespace TraderForge.API.Mappers;

public static class AdminPlanMappers
{
    public static CreatePlanCommand ToCommand(this CreatePlanRequest request)
    {
        return new CreatePlanCommand
        {
            Name = request.Name,
            MonthlyPrice = request.MonthlyPrice,
            InitialVirtualBalance = request.InitialVirtualBalance,
            MaxActiveStrategies = request.MaxActiveStrategies,
            MaxActiveAssets = request.MaxActiveAssets,
            CanModifyVirtualBalance = request.CanModifyVirtualBalance
        };
    }

    public static UpdatePlanCommand ToCommand(this UpdatePlanRequest request, Guid planId)
    {
        return new UpdatePlanCommand
        {
            PlanId = planId,
            Name = request.Name,
            MonthlyPrice = request.MonthlyPrice,
            InitialVirtualBalance = request.InitialVirtualBalance,
            MaxActiveStrategies = request.MaxActiveStrategies,
            MaxActiveAssets = request.MaxActiveAssets,
            CanModifyVirtualBalance = request.CanModifyVirtualBalance
        };
    }
}
