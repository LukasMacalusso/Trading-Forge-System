using FluentAssertions;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Enums;
using Xunit;

namespace TraderForge.Domain.Tests;

public class StrategyExecutionTests
{
    [Fact]
    public void Constructor_Initializes_Correctly()
    {
        var strategyId = Guid.NewGuid();
        var exec = new StrategyExecution(strategyId);

        exec.Id.Should().NotBeEmpty();
        exec.StrategyId.Should().Be(strategyId);
        exec.Status.Should().Be(ExecutionStatus.Running);
        exec.StartedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        exec.LastActivityAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        exec.CompletedAt.Should().BeNull();
    }

    [Fact]
    public void Complete_SetsStatusToCompleted_AndUpdatesDate()
    {
        var exec = new StrategyExecution(Guid.NewGuid());
        exec.Complete();
        exec.Status.Should().Be(ExecutionStatus.Completed);
        exec.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Fail_SetsStatusToFailed_AndUpdatesDate()
    {
        var exec = new StrategyExecution(Guid.NewGuid());
        exec.Fail();
        exec.Status.Should().Be(ExecutionStatus.Failed);
        exec.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Pause_SetsStatusToPaused_AndUpdatesActivity()
    {
        var exec = new StrategyExecution(Guid.NewGuid());
        exec.Pause();
        exec.Status.Should().Be(ExecutionStatus.Paused);
    }

    [Fact]
    public void Expire_SetsStatusToExpired()
    {
        var exec = new StrategyExecution(Guid.NewGuid());
        exec.Expire();
        exec.Status.Should().Be(ExecutionStatus.Expired);
    }

    [Fact]
    public void UpdateProgress_UpdatesActivityAndNode()
    {
        var exec = new StrategyExecution(Guid.NewGuid());
        var nodeId = Guid.NewGuid();
        exec.UpdateProgress(nodeId, "test-flag");
        exec.CurrentNodeId.Should().Be(nodeId);
        exec.CurrentFlag.Should().Be("test-flag");
    }
}
