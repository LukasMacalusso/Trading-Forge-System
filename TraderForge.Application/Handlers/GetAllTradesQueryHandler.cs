using TraderForge.Application.DTOs.Queries; // Importas la carpeta donde pusiste tu Query
using TraderForge.Domain.Common;
using TraderForge.Domain.Repositories;

namespace TraderForge.Application.Handlers;

public class GetAllTradesQueryHandler
{
    private readonly ITraderRepository _traderRepository;

    public GetAllTradesQueryHandler(ITraderRepository traderRepository)
    {
        _traderRepository = traderRepository;
    }

    public async Task<ResultGeneric<IEnumerable<object>>> HandleAsync(GetAllTradesQuery query)
    {
        var traders = await _traderRepository.GetAllIncludeSubPlanAsync();
        
        var response = traders.Select(t => new
        {
            t.Id,
            t.Email,
            t.UserName,
            t.IsSuspended,
            t.SuspensionReason,
            PlanId = t.Subscription?.SubscriptionPlanId
        });

        return ResultGeneric<IEnumerable<object>>.Success(response);
    }
}