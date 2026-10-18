using TraderForge.Domain.Common;
using TraderForge.Application.DTOs;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Repositories;

namespace TraderForge.Application.Handlers;

public class AddBotEdgeCommandHandler
{
    private readonly IBotEdgeRepository _edgeRepository;
    private readonly IBotNodeRepository _nodeRepository;

    public AddBotEdgeCommandHandler(IBotEdgeRepository edgeRepository, IBotNodeRepository nodeRepository)
    {
        _edgeRepository = edgeRepository;
        _nodeRepository = nodeRepository;
    }

    public async Task<ResultGeneric<Guid>> HandleAsync(AddBotEdgeCommand command)
    {
        try
        {
            var source = await _nodeRepository.GetByIdAsync(command.SourceNodeId);
            var target = await _nodeRepository.GetByIdAsync(command.TargetNodeId);

            if (source == null)
                return ResultGeneric<Guid>.Failure("Source node not found.");
            if (target == null)
                return ResultGeneric<Guid>.Failure("Target node not found.");
            if (source.StrategyId != command.StrategyId || target.StrategyId != command.StrategyId)
                return ResultGeneric<Guid>.Failure("Nodes must belong to the same strategy.");

            var edge = new BotEdge(command.StrategyId, command.SourceNodeId, command.SourcePort, command.TargetNodeId);
            await _edgeRepository.AddAsync(edge);
            await _edgeRepository.SaveChangesAsync();

            return ResultGeneric<Guid>.Success(edge.Id);
        }
        catch (Exception ex)
        {
            return ResultGeneric<Guid>.Failure(ex.Message);
        }
    }
}
