using FluentAssertions;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Enums;
using Xunit;

namespace TraderForge.Domain.Tests;

public class BotNodeTests
{
    [Fact]
    public void Constructor_InitializesProperties_Correctly()
    {
        // Arrange
        var strategyId = Guid.NewGuid();
        var type = BotNodeType.Condition;
        var name = "My Node";
        var config = "{ \"key\": \"value\" }";
        var posX = 100.5;
        var posY = 200.5;

        // Act
        var node = new BotNode(strategyId, type, name, config, posX, posY);

        // Assert
        node.Id.Should().NotBeEmpty();
        node.StrategyId.Should().Be(strategyId);
        node.Type.Should().Be(type);
        node.Name.Should().Be(name);
        node.Config.Should().Be(config);
        node.PositionX.Should().Be(posX);
        node.PositionY.Should().Be(posY);
        node.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Update_ModifiesProperties_Correctly()
    {
        var node = new BotNode(Guid.NewGuid(), BotNodeType.Action, "Old", "{}", 0, 0);

        node.Update("New", "{\"a\":1}", 10, 20);

        node.Name.Should().Be("New");
        node.Config.Should().Be("{\"a\":1}");
        node.PositionX.Should().Be(10);
        node.PositionY.Should().Be(20);
    }

    [Fact]
    public void ActivateDeactivate_TogglesIsActive()
    {
        var node = new BotNode(Guid.NewGuid(), BotNodeType.Action, "Test", "{}", 0, 0);

        node.Deactivate();
        node.IsActive.Should().BeFalse();

        node.Activate();
        node.IsActive.Should().BeTrue();
    }
}
