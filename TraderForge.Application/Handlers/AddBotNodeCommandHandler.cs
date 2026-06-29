using TraderForge.Domain.Common;
using TraderForge.Application.DTOs;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Repositories;

namespace TraderForge.Application.Handlers;

public class AddBotNodeCommandHandler
{
    private readonly IBotNodeRepository _nodeRepository;

    public AddBotNodeCommandHandler(IBotNodeRepository nodeRepository)
    {
        _nodeRepository = nodeRepository;
    }

    public async Task<ResultGeneric<Guid>> HandleAsync(AddBotNodeCommand command)
    {
        try
        {
            var node = new BotNode(
                command.StrategyId,
                command.Type,
                command.Name,
                command.Config,
                command.PositionX,
                command.PositionY);

            await _nodeRepository.AddAsync(node);
            await _nodeRepository.SaveChangesAsync();

            return ResultGeneric<Guid>.Success(node.Id);
        }
        catch (Exception ex)
        {
            return ResultGeneric<Guid>.Failure(ex.Message);
        }
    }
}
