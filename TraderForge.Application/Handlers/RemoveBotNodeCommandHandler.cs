using TraderForge.Domain.Common;
using TraderForge.Application.DTOs;
using TraderForge.Domain.Repositories;

namespace TraderForge.Application.Handlers;

public class RemoveBotNodeCommandHandler
{
    private readonly IBotNodeRepository _nodeRepository;

    public RemoveBotNodeCommandHandler(IBotNodeRepository nodeRepository)
    {
        _nodeRepository = nodeRepository;
    }

    public async Task<Result> HandleAsync(RemoveBotNodeCommand command)
    {
        try
        {
            var node = await _nodeRepository.GetByIdAsync(command.Id);
            if (node == null)
                return Result.Failure("BotNode not found.");

            _nodeRepository.Remove(node);
            await _nodeRepository.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
