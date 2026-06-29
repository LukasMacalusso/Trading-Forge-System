using TraderForge.API.Requests;
using TraderForge.Domain.Enums;

namespace TraderForge.API.Tests;

public class BotNodeRequestsTests
{
    [Fact]
    public void AddBotNodeRequest_DefaultValues()
    {
        var request = new AddBotNodeRequest();

        Assert.Equal(BotNodeType.Trigger, request.Type);
        Assert.Equal(string.Empty, request.Name);
        Assert.Equal("{}", request.Config);
        Assert.Equal(0, request.PositionX);
        Assert.Equal(0, request.PositionY);
    }

    [Fact]
    public void AddBotNodeRequest_CanSetAllProperties()
    {
        var request = new AddBotNodeRequest
        {
            Type = BotNodeType.Action,
            Name = "Buy Order",
            Config = """{"size":1.0}""",
            PositionX = 100.5,
            PositionY = 200.5
        };

        Assert.Equal(BotNodeType.Action, request.Type);
        Assert.Equal("Buy Order", request.Name);
        Assert.Equal("""{"size":1.0}""", request.Config);
        Assert.Equal(100.5, request.PositionX);
        Assert.Equal(200.5, request.PositionY);
    }

    [Fact]
    public void UpdateBotNodeRequest_DefaultValues()
    {
        var request = new UpdateBotNodeRequest();

        Assert.Equal(string.Empty, request.Name);
        Assert.Equal("{}", request.Config);
        Assert.Equal(0, request.PositionX);
        Assert.Equal(0, request.PositionY);
    }

    [Fact]
    public void UpdateBotNodeRequest_CanSetAllProperties()
    {
        var request = new UpdateBotNodeRequest
        {
            Name = "Renamed",
            Config = """{"new":true}""",
            PositionX = 50.0,
            PositionY = 75.0
        };

        Assert.Equal("Renamed", request.Name);
        Assert.Equal("""{"new":true}""", request.Config);
        Assert.Equal(50.0, request.PositionX);
        Assert.Equal(75.0, request.PositionY);
    }

    [Fact]
    public void AddBotEdgeRequest_DefaultValues()
    {
        var request = new AddBotEdgeRequest();

        Assert.Equal(Guid.Empty, request.SourceNodeId);
        Assert.Equal(NodePort.Out, request.SourcePort);
        Assert.Equal(Guid.Empty, request.TargetNodeId);
    }

    [Fact]
    public void AddBotEdgeRequest_CanSetAllProperties()
    {
        var srcId = Guid.NewGuid();
        var tgtId = Guid.NewGuid();
        var request = new AddBotEdgeRequest
        {
            SourceNodeId = srcId,
            SourcePort = NodePort.False,
            TargetNodeId = tgtId
        };

        Assert.Equal(srcId, request.SourceNodeId);
        Assert.Equal(NodePort.False, request.SourcePort);
        Assert.Equal(tgtId, request.TargetNodeId);
    }

    [Fact]
    public void CreateStrategyRequest_DefaultValues()
    {
        var request = new CreateStrategyRequest();
        Assert.Equal(string.Empty, request.Name);
    }

    [Fact]
    public void CreateStrategyRequest_CanSetName()
    {
        var request = new CreateStrategyRequest { Name = "My Bot Strategy" };
        Assert.Equal("My Bot Strategy", request.Name);
    }

    [Fact]
    public void UpdateStrategyStateRequest_DefaultValues()
    {
        var request = new UpdateStrategyStateRequest();
        Assert.False(request.IsActive);
    }

    [Fact]
    public void UpdateStrategyStateRequest_CanSetIsActive()
    {
        var request = new UpdateStrategyStateRequest { IsActive = true };
        Assert.True(request.IsActive);
    }
}
