using TraderForge.API.Requests;
using TraderForge.Application.DTOs;

namespace TraderForge.API.Mappers;

public static class PortfolioMappers
{
    public static CreateStrategyCommand ToCommand(this CreateStrategyRequest request, string traderId)
    {
        return new CreateStrategyCommand
        {
            TraderId = traderId,
            Name = request.Name
        };
    }

    public static UpdateStrategyStateCommand ToCommand(this UpdateStrategyStateRequest request, Guid strategyId)
    {
        return new UpdateStrategyStateCommand
        {
            StrategyId = strategyId,
            IsActive = request.IsActive
        };
    }

    public static AddPositionCommand ToCommand(this AddPortfolioAssetRequest request, string traderId)
    {
        return new AddPositionCommand
        {
            TraderId = traderId,
            Symbol = request.Symbol,
            Quantity = request.Quantity,
            EntryPrice = request.EntryPrice
        };
    }
}
