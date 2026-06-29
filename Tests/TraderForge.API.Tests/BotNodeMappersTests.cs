using TraderForge.API.Mappers;
using TraderForge.API.Requests;
using TraderForge.Application.DTOs;
using TraderForge.Domain.Enums;

namespace TraderForge.API.Tests;

public class BotNodeMappersTests
{
    [Fact]
    public void AddBotNodeRequest_ToCommand_MapsAllProperties()
    {
        var strategyId = Guid.NewGuid();
        var request = new AddBotNodeRequest
        {
            Type = BotNodeType.Trigger,
            Name = "Price Trigger",
            Config = """{"symbol":"BTCUSDT"}""",
            PositionX = 150.5,
            PositionY = 250.7
        };

        var command = request.ToCommand(strategyId);

        Assert.Equal(strategyId, command.StrategyId);
        Assert.Equal(BotNodeType.Trigger, command.Type);
        Assert.Equal("Price Trigger", command.Name);
        Assert.Equal("""{"symbol":"BTCUSDT"}""", command.Config);
        Assert.Equal(150.5, command.PositionX);
        Assert.Equal(250.7, command.PositionY);
    }

    [Fact]
    public void UpdateBotNodeRequest_ToCommand_MapsAllProperties()
    {
        var nodeId = Guid.NewGuid();
        var request = new UpdateBotNodeRequest
        {
            Name = "Updated Node",
            Config = """{"threshold":0.5}""",
            PositionX = 300.0,
            PositionY = 400.0
        };

        var command = request.ToCommand(nodeId);

        Assert.Equal(nodeId, command.Id);
        Assert.Equal("Updated Node", command.Name);
        Assert.Equal("""{"threshold":0.5}""", command.Config);
        Assert.Equal(300.0, command.PositionX);
        Assert.Equal(400.0, command.PositionY);
    }

    [Fact]
    public void AddBotEdgeRequest_ToCommand_MapsAllProperties()
    {
        var strategyId = Guid.NewGuid();
        var sourceNodeId = Guid.NewGuid();
        var targetNodeId = Guid.NewGuid();
        var request = new AddBotEdgeRequest
        {
            SourceNodeId = sourceNodeId,
            SourcePort = NodePort.True,
            TargetNodeId = targetNodeId
        };

        var command = request.ToCommand(strategyId);

        Assert.Equal(strategyId, command.StrategyId);
        Assert.Equal(sourceNodeId, command.SourceNodeId);
        Assert.Equal(NodePort.True, command.SourcePort);
        Assert.Equal(targetNodeId, command.TargetNodeId);
    }
}
