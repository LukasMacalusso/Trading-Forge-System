using TraderForge.Application.DTOs;
using TraderForge.Domain.Common;
using TraderForge.Domain.Repositories;

namespace TraderForge.Application.Handlers;

public class GetPendingOperationsQueryHandler
{
    private readonly IPendingOperationRepository _repo;

    public GetPendingOperationsQueryHandler(IPendingOperationRepository repo)
    {
        _repo = repo;
    }

    public async Task<ResultGeneric<IEnumerable<object>>> HandleAsync(GetPendingOperationsQuery query)
    {
        var pending = await _repo.GetPendingByTraderIdAsync(query.TraderId);

        var result = pending.Select(p => new
        {
            p.Id,
            FlowId = p.StrategyId,
            FlowName = p.StrategyName,
            p.Symbol,
            p.Action,
            p.Quantity,
            p.CurrentPrice,
            p.ConditionMetAt,
            p.ExpiresAt
        });

        return ResultGeneric<IEnumerable<object>>.Success(result);
    }
}
