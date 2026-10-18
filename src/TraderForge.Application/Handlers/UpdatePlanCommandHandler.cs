using TraderForge.Domain.Common;
using TraderForge.Application.DTOs;
using TraderForge.Domain.Repositories;

namespace TraderForge.Application.Handlers;

public class UpdatePlanCommandHandler
{
    private readonly ISubscriptionPlanRepository _planRepository;

    public UpdatePlanCommandHandler(ISubscriptionPlanRepository planRepository)
    {
        _planRepository = planRepository;
    }

    public async Task<Result> HandleAsync(UpdatePlanCommand command)
    {
        try
        {
            return await UpdatePlan(command);
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }
    }

    private async Task<Result> UpdatePlan(UpdatePlanCommand command)
    {
        var plan = await _planRepository.GetByIdAsync(command.PlanId);
        if (plan is null)
            return Result.Failure("Subscription plan not found.");

        plan.Update(
            command.Name,
            command.MonthlyPrice,
            command.InitialVirtualBalance,
            command.MaxActiveStrategies,
            command.MaxActiveAssets,
            command.CanModifyVirtualBalance
        );

        await _planRepository.SaveChangesAsync();
        return Result.Success();
    }
}
