using TraderForge.API.Requests;
using TraderForge.Application.DTOs;

namespace TraderForge.API.Mappers;

public static class BotNodeMappers
{
    public static AddBotNodeCommand ToCommand(this AddBotNodeRequest request, Guid strategyId) => new()
    {
        StrategyId = strategyId,
        Type = request.Type,
        Name = request.Name,
        Config = request.Config,
        PositionX = request.PositionX,
        PositionY = request.PositionY
    };

    public static UpdateBotNodeCommand ToCommand(this UpdateBotNodeRequest request, Guid nodeId) => new()
    {
        Id = nodeId,
        Name = request.Name,
        Config = request.Config,
        PositionX = request.PositionX,
        PositionY = request.PositionY
    };

    public static AddBotEdgeCommand ToCommand(this AddBotEdgeRequest request, Guid strategyId) => new()
    {
        StrategyId = strategyId,
        SourceNodeId = request.SourceNodeId,
        SourcePort = request.SourcePort,
        TargetNodeId = request.TargetNodeId
    };
}
