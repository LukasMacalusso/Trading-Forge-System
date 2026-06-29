using TraderForge.API.Mappers;
using TraderForge.API.Requests;
using TraderForge.Application.DTOs;

namespace TraderForge.API.Tests;

public class PortfolioMappersTests
{
    [Fact]
    public void CreateStrategyRequest_ToCommand_MapsAllProperties()
    {
        var traderId = "test-trader-id";
        var request = new CreateStrategyRequest { Name = "My Strategy" };

        var command = request.ToCommand(traderId);

        Assert.Equal(traderId, command.TraderId);
        Assert.Equal("My Strategy", command.Name);
    }

    [Fact]
    public void UpdateStrategyStateRequest_ToCommand_MapsAllProperties()
    {
        var strategyId = Guid.NewGuid();
        var request = new UpdateStrategyStateRequest { IsActive = true };

        var command = request.ToCommand(strategyId);

        Assert.Equal(strategyId, command.StrategyId);
        Assert.True(command.IsActive);
    }

    [Fact]
    public void BuyPositionRequest_ToCommand_MapsAllProperties()
    {
        var traderId = "test-trader-id";
        var request = new BuyPositionRequest { Symbol = "BTCUSDT", Quantity = 0.5m };

        var command = request.ToCommand(traderId);

        Assert.Equal(traderId, command.TraderId);
        Assert.Equal("BTCUSDT", command.Symbol);
        Assert.Equal(0.5m, command.Quantity);
    }

    [Fact]
    public void SellPositionRequest_ToCommand_MapsAllProperties()
    {
        var positionId = Guid.NewGuid();
        var request = new SellPositionRequest { Quantity = 0.25m };

        var command = request.ToCommand(positionId);

        Assert.Equal(positionId, command.PositionId);
        Assert.Equal(0.25m, command.Quantity);
    }
}
