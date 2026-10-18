using TraderForge.API.Requests;
using TraderForge.Application.DTOs;

namespace TraderForge.API.Mappers;

public static class PortfolioMappers
{
    public static CreateStrategyCommand ToCommand(this CreateStrategyRequest request, string traderId) => new()
    {
        TraderId = traderId,
        Name = request.Name
    };

    public static UpdateStrategyStateCommand ToCommand(this UpdateStrategyStateRequest request, Guid strategyId) => new()
    {
        StrategyId = strategyId,
        IsActive = request.IsActive
    };

    public static BuyPositionCommand ToCommand(this BuyPositionRequest request, string traderId) => new()
    {
        TraderId = traderId,
        Symbol = request.Symbol,
        Quantity = request.Quantity
    };

    public static SellPositionCommand ToCommand(this SellPositionRequest request, Guid positionId) => new()
    {
        PositionId = positionId,
        Quantity = request.Quantity
    };
}
