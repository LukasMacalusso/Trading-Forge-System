using FluentAssertions;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Enums;
using Xunit;

namespace TraderForge.Domain.Tests;

public class BotEdgeTests
{
    [Fact]
    public void Constructor_InitializesProperties_Correctly()
    {
        // Arrange
        var strategyId = Guid.NewGuid();
        var sourceNodeId = Guid.NewGuid();
        var sourcePort = NodePort.True;
        var targetNodeId = Guid.NewGuid();

        // Act
        var edge = new BotEdge(strategyId, sourceNodeId, sourcePort, targetNodeId);

        // Assert
        edge.Id.Should().NotBeEmpty();
        edge.StrategyId.Should().Be(strategyId);
        edge.SourceNodeId.Should().Be(sourceNodeId);
        edge.SourcePort.Should().Be(sourcePort);
        edge.TargetNodeId.Should().Be(targetNodeId);
    }
}
