using TraderForge.Domain.Common;
using TraderForge.Application.DTOs;
using TraderForge.Domain.Repositories;

namespace TraderForge.Application.Handlers;

public class RemoveBotEdgeCommandHandler
{
    private readonly IBotEdgeRepository _edgeRepository;

    public RemoveBotEdgeCommandHandler(IBotEdgeRepository edgeRepository)
    {
        _edgeRepository = edgeRepository;
    }

    public async Task<Result> HandleAsync(RemoveBotEdgeCommand command)
    {
        try
        {
            var edge = await _edgeRepository.GetByIdAsync(command.Id);
            if (edge == null)
                return Result.Failure("BotEdge not found.");

            _edgeRepository.Remove(edge);
            await _edgeRepository.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
