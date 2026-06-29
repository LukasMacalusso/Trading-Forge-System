using TraderForge.Domain.Common;
using TraderForge.Application.DTOs;
using TraderForge.Domain.Repositories;

namespace TraderForge.Application.Handlers;

public class UpdateBotNodeCommandHandler
{
    private readonly IBotNodeRepository _nodeRepository;

    public UpdateBotNodeCommandHandler(IBotNodeRepository nodeRepository)
    {
        _nodeRepository = nodeRepository;
    }

    public async Task<Result> HandleAsync(UpdateBotNodeCommand command)
    {
        try
        {
            var node = await _nodeRepository.GetByIdAsync(command.Id);
            if (node == null)
                return Result.Failure("BotNode not found.");

            node.Update(command.Name, command.Config, command.PositionX, command.PositionY);
            _nodeRepository.Update(node);
            await _nodeRepository.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
