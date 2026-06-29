using TraderForge.Application.DTOs.Queries; 
using TraderForge.Application.DTOs.Responses;
using TraderForge.Domain.Common;
using TraderForge.Domain.Repositories;

namespace TraderForge.Application.Handlers;

public class GetAllTradersQueryHandler
{
    private readonly ITraderRepository _traderRepository;

    public GetAllTradersQueryHandler(ITraderRepository traderRepository)
    {
        _traderRepository = traderRepository;
    }

    public async Task<ResultGeneric<IEnumerable<TraderSummaryResponse>>> HandleAsync(GetAllTradersQuery query)
    {
        var traders = await _traderRepository.GetAllIncludeSubPlanAsync();
        
        var response = traders.Select(t => new TraderSummaryResponse
        {
            Id = t.Id,
            Email = t.Email,
            IsSuspended = t.IsSuspended,
            SuspensionReason = t.SuspensionReason,
            ActivePlanId = t.Subscription?.SubscriptionPlanId.ToString()
        });

        return ResultGeneric<IEnumerable<TraderSummaryResponse>>.Success(response);
    }
}