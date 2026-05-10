using TraderForge.Application.Common;
using TraderForge.Application.DTOs;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Interfaces;

namespace TraderForge.Application.Handlers;

public class CreatePlanCommandHandler
{
    private readonly ISubscriptionPlanRepository _planRepository;

    public CreatePlanCommandHandler(ISubscriptionPlanRepository planRepository)
    {
        _planRepository = planRepository;
    }

    public async Task<Result> HandleAsync(CreatePlanCommand command)
    {
        try
        {
            return await CreateSubscriptionPlan(command);
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }
    }

    private async Task<Result> CreateSubscriptionPlan(CreatePlanCommand command)
    {
        var plan = new SubscriptionPlan(
            Guid.NewGuid(),
            command.Name,
            command.MonthlyPrice,
            command.InitialVirtualBalance,
            command.MaxActiveStrategies,
            command.MaxActiveAssets,
            command.CanModifyVirtualBalance
        );

        await _planRepository.AddAsync(plan);
        return Result.Success();
    }
}
