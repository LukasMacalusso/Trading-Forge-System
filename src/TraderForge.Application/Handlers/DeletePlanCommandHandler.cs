using TraderForge.Domain.Common;
using TraderForge.Domain.Repositories;

namespace TraderForge.Application.Handlers;

public class DeletePlanCommandHandler
{
    private readonly ISubscriptionPlanRepository _planRepository;

    public DeletePlanCommandHandler(ISubscriptionPlanRepository planRepository)
    {
        _planRepository = planRepository;
    }

    public async Task<Result> HandleAsync(Guid planId)
    {
        try
        {
            return await DeletePlan(planId);
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }
    }

    private async Task<Result> DeletePlan(Guid planId)
    {
        var plan = await _planRepository.GetByIdAsync(planId);
        if (plan is null)
            return Result.Failure("Subscription plan not found.");

        await _planRepository.DeleteAsync(planId);
        return Result.Success();
    }
}
