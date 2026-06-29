using Microsoft.Extensions.DependencyInjection;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Enums;
using TraderForge.Domain.Events;
using TraderForge.Domain.Repositories;
using TraderForge.Domain.Services;
using TraderForge.Infrastructure.Services;

namespace TraderForge.Infrastructure.Tests;

public class BotGraphRunnerTests
{
    [Fact]
    public async Task TriggerToAction_CompletesSuccessfully()
    {
        var (s, trigger, _) = CreateBaseStrategy();
        var runner = CreateRunner(s, out var executionRepo);

        await runner.ExecuteAsync(MakePriceEvent("BTCUSDT", 50000), trigger.Id, CancellationToken.None);

        var execution = Assert.Single(await executionRepo.GetAllExecutionsAsync());
        Assert.Equal(ExecutionStatus.Completed, execution.Status);
    }

    [Fact]
    public async Task TriggerToConditionTrueToAction_Completes()
    {
        var (s, trigger, _) = CreateBaseStrategy();
        var cond = new BotNode(s.Id, BotNodeType.Condition, "Price > 40000",
            """{"indicator":"price","operator":"greater_than","value":40000}""", 200, 200);
        var act = new BotNode(s.Id, BotNodeType.Action, "Buy",
            """{"type":"buy"}""", 200, 400);
        s.BotNodes.Add(cond); s.BotNodes.Add(act);
        s.BotEdges.Add(new BotEdge(s.Id, trigger.Id, NodePort.Out, cond.Id));
        s.BotEdges.Add(new BotEdge(s.Id, cond.Id, NodePort.True, act.Id));
        var runner = CreateRunner(s, out var executionRepo);

        await runner.ExecuteAsync(MakePriceEvent("BTCUSDT", 50000), trigger.Id, CancellationToken.None);

        var execution = Assert.Single(await executionRepo.GetAllExecutionsAsync());
        Assert.Equal(ExecutionStatus.Completed, execution.Status);
    }

    [Fact]
    public async Task TriggerToConditionFalseToAction_Completes()
    {
        var (s, trigger, _) = CreateBaseStrategy();
        var cond = new BotNode(s.Id, BotNodeType.Condition, "Price > 60000",
            """{"indicator":"price","operator":"greater_than","value":60000}""", 200, 200);
        var act = new BotNode(s.Id, BotNodeType.Action, "Sell",
            """{"type":"sell"}""", 200, 400);
        s.BotNodes.Add(cond); s.BotNodes.Add(act);
        s.BotEdges.Add(new BotEdge(s.Id, trigger.Id, NodePort.Out, cond.Id));
        s.BotEdges.Add(new BotEdge(s.Id, cond.Id, NodePort.False, act.Id));
        var runner = CreateRunner(s, out var executionRepo);

        await runner.ExecuteAsync(MakePriceEvent("BTCUSDT", 50000), trigger.Id, CancellationToken.None);

        var execution = Assert.Single(await executionRepo.GetAllExecutionsAsync());
        Assert.Equal(ExecutionStatus.Completed, execution.Status);
    }

    [Fact]
    public async Task ChainedConditions_BothTrue_ReachesAction()
    {
        var (s, trigger, _) = CreateBaseStrategy();
        var c1 = new BotNode(s.Id, BotNodeType.Condition, "Price > 40000", """{"indicator":"price","operator":"greater_than","value":40000}""", 200, 200);
        var c2 = new BotNode(s.Id, BotNodeType.Condition, "Price < 60000", """{"indicator":"price","operator":"less_than","value":60000}""", 200, 300);
        var act = new BotNode(s.Id, BotNodeType.Action, "Hold", """{"type":"hold"}""", 200, 400);
        s.BotNodes.Add(c1); s.BotNodes.Add(c2); s.BotNodes.Add(act);
        s.BotEdges.Add(new BotEdge(s.Id, trigger.Id, NodePort.Out, c1.Id));
        s.BotEdges.Add(new BotEdge(s.Id, c1.Id, NodePort.True, c2.Id));
        s.BotEdges.Add(new BotEdge(s.Id, c2.Id, NodePort.True, act.Id));
        var runner = CreateRunner(s, out var executionRepo);

        await runner.ExecuteAsync(MakePriceEvent("BTCUSDT", 50000), trigger.Id, CancellationToken.None);

        var execution = Assert.Single(await executionRepo.GetAllExecutionsAsync());
        Assert.Equal(ExecutionStatus.Completed, execution.Status);
    }

    [Fact]
    public async Task ChainedConditions_FirstFalse_TakesFalseBranch()
    {
        var (s, trigger, _) = CreateBaseStrategy();
        var c1 = new BotNode(s.Id, BotNodeType.Condition, "Price > 60000", """{"indicator":"price","operator":"greater_than","value":60000}""", 200, 200);
        var act = new BotNode(s.Id, BotNodeType.Action, "Buy", """{"type":"buy"}""", 200, 400);
        s.BotNodes.Add(c1); s.BotNodes.Add(act);
        s.BotEdges.Add(new BotEdge(s.Id, trigger.Id, NodePort.Out, c1.Id));
        s.BotEdges.Add(new BotEdge(s.Id, c1.Id, NodePort.False, act.Id));
        var runner = CreateRunner(s, out var executionRepo);

        await runner.ExecuteAsync(MakePriceEvent("BTCUSDT", 50000), trigger.Id, CancellationToken.None);

        var execution = Assert.Single(await executionRepo.GetAllExecutionsAsync());
        Assert.Equal(ExecutionStatus.Completed, execution.Status);
    }

    [Fact]
    public async Task MultipleTriggers_BothFire_IndependentExecutions()
    {
        var (s, btcTrigger, ethTrigger, _) = CreateTwoTriggerStrategy();
        var runner = CreateRunner(s, out var executionRepo);

        await runner.ExecuteAsync(MakePriceEvent("BTCUSDT", 50000), btcTrigger.Id, CancellationToken.None);
        await runner.ExecuteAsync(MakePriceEvent("ETHUSDT", 3000), ethTrigger.Id, CancellationToken.None);

        var executions = await executionRepo.GetAllExecutionsAsync();
        Assert.Equal(2, executions.Count);
        Assert.All(executions, e => Assert.Equal(ExecutionStatus.Completed, e.Status));
    }

    [Fact]
    public void HandlesSymbol_ReturnsCorrectly()
    {
        var (s, _, _, _) = CreateTwoTriggerStrategy();
        var runner = CreateRunner(s, out _);

        Assert.True(runner.HandlesSymbol("BTCUSDT"));
        Assert.True(runner.HandlesSymbol("ETHUSDT"));
        Assert.False(runner.HandlesSymbol("SOLUSDT"));
    }

    [Fact]
    public void GetTriggersForSymbol_ReturnsMatchingTriggers()
    {
        var (s, btcTrigger, ethTrigger, _) = CreateTwoTriggerStrategy();
        var runner = CreateRunner(s, out _);

        Assert.Equal(btcTrigger.Id, Assert.Single(runner.GetTriggersForSymbol("BTCUSDT")));
        Assert.Equal(ethTrigger.Id, Assert.Single(runner.GetTriggersForSymbol("ETHUSDT")));
        Assert.Empty(runner.GetTriggersForSymbol("SOLUSDT"));
    }

    [Fact]
    public async Task NoOutgoingEdge_SilentlyStops()
    {
        var (s, trigger, _) = CreateBaseStrategy();
        var runner = CreateRunner(s, out var executionRepo);

        await runner.ExecuteAsync(MakePriceEvent("BTCUSDT", 50000), trigger.Id, CancellationToken.None);

        var execution = Assert.Single(await executionRepo.GetAllExecutionsAsync());
        Assert.Equal(ExecutionStatus.Completed, execution.Status);
    }

    [Fact]
    public async Task Dispose_PreventsExecution()
    {
        var (s, trigger, _) = CreateBaseStrategy();
        var runner = CreateRunner(s, out var executionRepo);
        runner.Dispose();

        await runner.ExecuteAsync(MakePriceEvent("BTCUSDT", 50000), trigger.Id, CancellationToken.None);

        Assert.Empty(await executionRepo.GetAllExecutionsAsync());
    }

    // ---- BotNode Entity Tests ----

    [Fact]
    public void BotNode_ActivateAndDeactivate_ToggleState()
    {
        var node = new BotNode(Guid.NewGuid(), BotNodeType.Trigger, "T", "{}", 0, 0);
        Assert.True(node.IsActive);
        node.Deactivate();
        Assert.False(node.IsActive);
        node.Activate();
        Assert.True(node.IsActive);
    }

    [Fact]
    public void BotNode_Update_ChangesProperties()
    {
        var node = new BotNode(Guid.NewGuid(), BotNodeType.Trigger, "Old", """{"symbol":"BTC"}""", 0, 0);
        node.Update("New", """{"symbol":"ETH"}""", 100, 200);
        Assert.Equal("New", node.Name);
        Assert.Contains("ETH", node.Config);
        Assert.Equal(100, node.PositionX);
        Assert.Equal(200, node.PositionY);
    }

    // ---- StrategyExecution Entity Tests ----

    [Fact]
    public void StrategyExecution_Lifecycle()
    {
        var sid = Guid.NewGuid();
        var exec = new StrategyExecution(sid);

        Assert.Equal(ExecutionStatus.Running, exec.Status);
        Assert.Equal(sid, exec.StrategyId);
        Assert.NotNull(exec.StartedAt);

        exec.Pause();
        Assert.Equal(ExecutionStatus.Paused, exec.Status);

        exec.Complete();
        Assert.Equal(ExecutionStatus.Completed, exec.Status);
        Assert.NotNull(exec.CompletedAt);
    }

    [Fact]
    public void StrategyExecution_FailAndExpire_SetCorrectStatus()
    {
        var exec = new StrategyExecution(Guid.NewGuid());
        exec.Fail();
        Assert.Equal(ExecutionStatus.Failed, exec.Status);
        Assert.NotNull(exec.CompletedAt);

        exec = new StrategyExecution(Guid.NewGuid());
        exec.Expire();
        Assert.Equal(ExecutionStatus.Expired, exec.Status);
        Assert.NotNull(exec.CompletedAt);
    }

    [Fact]
    public void StrategyExecution_UpdateProgress_SetsCurrentNode()
    {
        var exec = new StrategyExecution(Guid.NewGuid());
        var nodeId = Guid.NewGuid();
        exec.UpdateProgress(nodeId, """{"price":50000}""");
        Assert.Equal(nodeId, exec.CurrentNodeId);
        Assert.Contains("50000", exec.CurrentFlag);
        Assert.NotNull(exec.LastActivityAt);
    }

    // ---- BotEdge Entity Tests ----

    [Fact]
    public void BotEdge_Creation_SetsProperties()
    {
        var sid = Guid.NewGuid();
        var src = Guid.NewGuid();
        var tgt = Guid.NewGuid();
        var edge = new BotEdge(sid, src, NodePort.True, tgt);

        Assert.Equal(sid, edge.StrategyId);
        Assert.Equal(src, edge.SourceNodeId);
        Assert.Equal(NodePort.True, edge.SourcePort);
        Assert.Equal(tgt, edge.TargetNodeId);
    }

    // ---- Strategy Entity New Methods Tests ----

    [Fact]
    public void Strategy_EngineActive_TogglesCorrectly()
    {
        var s = new Strategy(Guid.NewGuid(), "Test", Guid.NewGuid());
        Assert.False(s.IsEngineActive);
        s.StartEngine();
        Assert.True(s.IsEngineActive);
        s.StopEngine();
        Assert.False(s.IsEngineActive);
    }

    // ---- Engine Service Tests ----

    [Fact]
    public async Task Engine_StartStrategy_AddsToActiveStrategies()
    {
        var (s, _, _) = CreateBaseStrategy();
        var engine = BuildEngine(s);
        await engine.StartStrategyAsync(s.Id);
        Assert.True(engine.IsStrategyRunning(s.Id));
    }

    [Fact]
    public async Task Engine_StopStrategy_RemovesFromActiveStrategies()
    {
        var (s, _, _) = CreateBaseStrategy();
        var engine = BuildEngine(s);
        await engine.StartStrategyAsync(s.Id);
        Assert.True(engine.IsStrategyRunning(s.Id));
        await engine.StopStrategyAsync(s.Id);
        Assert.False(engine.IsStrategyRunning(s.Id));
    }

    [Fact]
    public async Task Engine_StopStrategy_WhenNotRunning_DoesNotThrow()
    {
        var engine = BuildEngine(null);
        var ex = await Record.ExceptionAsync(() => engine.StopStrategyAsync(Guid.NewGuid()));
        Assert.Null(ex);
    }

    // ---- MarketDataEventBus Tests ----

    [Fact]
    public async Task EventBus_PublishAndRead_DeliversEvent()
    {
        var bus = new MarketDataEventBus();
        var evt = new MarketPriceEvent("BTCUSDT", 65432.10m, DateTime.UtcNow);
        bus.Publish(evt);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
        var received = new List<MarketPriceEvent>();
        await foreach (var e in bus.ReadAllAsync(cts.Token)) { received.Add(e); break; }

        Assert.Equal(evt.Symbol, Assert.Single(received).Symbol);
    }

    [Fact]
    public async Task EventBus_MultiplePublishes_DeliversInOrder()
    {
        var bus = new MarketDataEventBus();
        bus.Publish(new MarketPriceEvent("BTCUSDT", 50000, DateTime.UtcNow));
        bus.Publish(new MarketPriceEvent("ETHUSDT", 3000, DateTime.UtcNow));
        bus.Publish(new MarketPriceEvent("SOLUSDT", 150, DateTime.UtcNow));

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
        var received = new List<MarketPriceEvent>();
        await foreach (var e in bus.ReadAllAsync(cts.Token)) { received.Add(e); if (received.Count == 3) break; }

        Assert.Equal(3, received.Count);
        Assert.Equal("BTCUSDT", received[0].Symbol);
        Assert.Equal("ETHUSDT", received[1].Symbol);
        Assert.Equal("SOLUSDT", received[2].Symbol);
    }

    [Fact]
    public async Task EventBus_CancellationToken_Throws()
    {
        var bus = new MarketDataEventBus();
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        await Assert.ThrowsAsync<TaskCanceledException>(async () =>
        {
            await foreach (var e in bus.ReadAllAsync(cts.Token)) { }
        });
    }

    // ---- Condition Evaluation Operator Tests ----

    [Fact]
    public async Task Condition_GreaterThanTrue_FollowsTrueBranch()
    {
        var (s, trigger, _) = CreateBaseStrategy();
        var cond = new BotNode(s.Id, BotNodeType.Condition, "Price > 40000",
            """{"indicator":"price","operator":"greater_than","value":40000}""", 200, 200);
        var actionTrue = new BotNode(s.Id, BotNodeType.Action, "Buy", """{"type":"buy"}""", 200, 400);
        var actionFalse = new BotNode(s.Id, BotNodeType.Action, "Sell", """{"type":"sell"}""", 400, 400);
        s.BotNodes.Add(cond); s.BotNodes.Add(actionTrue); s.BotNodes.Add(actionFalse);
        s.BotEdges.Clear();
        s.BotEdges.Add(new BotEdge(s.Id, trigger.Id, NodePort.Out, cond.Id));
        s.BotEdges.Add(new BotEdge(s.Id, cond.Id, NodePort.True, actionTrue.Id));
        s.BotEdges.Add(new BotEdge(s.Id, cond.Id, NodePort.False, actionFalse.Id));
        var runner = CreateRunner(s, out var executionRepo);

        await runner.ExecuteAsync(MakePriceEvent("BTCUSDT", 50000), trigger.Id, CancellationToken.None);

        var execution = Assert.Single(await executionRepo.GetAllExecutionsAsync());
        Assert.Equal(ExecutionStatus.Completed, execution.Status);
        Assert.Equal(actionTrue.Id, execution.CurrentNodeId);
    }

    [Fact]
    public async Task Condition_GreaterThanFalse_FollowsFalseBranch()
    {
        var (s, trigger, _) = CreateBaseStrategy();
        var cond = new BotNode(s.Id, BotNodeType.Condition, "Price > 60000",
            """{"indicator":"price","operator":"greater_than","value":60000}""", 200, 200);
        var actionTrue = new BotNode(s.Id, BotNodeType.Action, "Buy", """{"type":"buy"}""", 200, 400);
        var actionFalse = new BotNode(s.Id, BotNodeType.Action, "Sell", """{"type":"sell"}""", 400, 400);
        s.BotNodes.Add(cond); s.BotNodes.Add(actionTrue); s.BotNodes.Add(actionFalse);
        s.BotEdges.Clear();
        s.BotEdges.Add(new BotEdge(s.Id, trigger.Id, NodePort.Out, cond.Id));
        s.BotEdges.Add(new BotEdge(s.Id, cond.Id, NodePort.True, actionTrue.Id));
        s.BotEdges.Add(new BotEdge(s.Id, cond.Id, NodePort.False, actionFalse.Id));
        var runner = CreateRunner(s, out var executionRepo);

        await runner.ExecuteAsync(MakePriceEvent("BTCUSDT", 50000), trigger.Id, CancellationToken.None);

        var execution = Assert.Single(await executionRepo.GetAllExecutionsAsync());
        Assert.Equal(ExecutionStatus.Completed, execution.Status);
        Assert.Equal(actionFalse.Id, execution.CurrentNodeId);
    }

    [Fact]
    public async Task Condition_LessThanTrue_FollowsTrueBranch()
    {
        var (s, trigger, _) = CreateBaseStrategy();
        var cond = new BotNode(s.Id, BotNodeType.Condition, "Price < 60000",
            """{"indicator":"price","operator":"less_than","value":60000}""", 200, 200);
        var actionTrue = new BotNode(s.Id, BotNodeType.Action, "Buy", """{"type":"buy"}""", 200, 400);
        var actionFalse = new BotNode(s.Id, BotNodeType.Action, "Sell", """{"type":"sell"}""", 400, 400);
        s.BotNodes.Add(cond); s.BotNodes.Add(actionTrue); s.BotNodes.Add(actionFalse);
        s.BotEdges.Clear();
        s.BotEdges.Add(new BotEdge(s.Id, trigger.Id, NodePort.Out, cond.Id));
        s.BotEdges.Add(new BotEdge(s.Id, cond.Id, NodePort.True, actionTrue.Id));
        s.BotEdges.Add(new BotEdge(s.Id, cond.Id, NodePort.False, actionFalse.Id));
        var runner = CreateRunner(s, out var executionRepo);

        await runner.ExecuteAsync(MakePriceEvent("BTCUSDT", 50000), trigger.Id, CancellationToken.None);

        var execution = Assert.Single(await executionRepo.GetAllExecutionsAsync());
        Assert.Equal(ExecutionStatus.Completed, execution.Status);
        Assert.Equal(actionTrue.Id, execution.CurrentNodeId);
    }

    [Fact]
    public async Task Condition_EqualToTrue_FollowsTrueBranch()
    {
        var (s, trigger, _) = CreateBaseStrategy();
        var cond = new BotNode(s.Id, BotNodeType.Condition, "Price == 50000",
            """{"indicator":"price","operator":"equal_to","value":50000}""", 200, 200);
        var actionTrue = new BotNode(s.Id, BotNodeType.Action, "Buy", """{"type":"buy"}""", 200, 400);
        var actionFalse = new BotNode(s.Id, BotNodeType.Action, "Sell", """{"type":"sell"}""", 400, 400);
        s.BotNodes.Add(cond); s.BotNodes.Add(actionTrue); s.BotNodes.Add(actionFalse);
        s.BotEdges.Clear();
        s.BotEdges.Add(new BotEdge(s.Id, trigger.Id, NodePort.Out, cond.Id));
        s.BotEdges.Add(new BotEdge(s.Id, cond.Id, NodePort.True, actionTrue.Id));
        s.BotEdges.Add(new BotEdge(s.Id, cond.Id, NodePort.False, actionFalse.Id));
        var runner = CreateRunner(s, out var executionRepo);

        await runner.ExecuteAsync(MakePriceEvent("BTCUSDT", 50000), trigger.Id, CancellationToken.None);

        var execution = Assert.Single(await executionRepo.GetAllExecutionsAsync());
        Assert.Equal(ExecutionStatus.Completed, execution.Status);
        Assert.Equal(actionTrue.Id, execution.CurrentNodeId);
    }

    [Fact]
    public async Task Condition_EqualToFalse_FollowsFalseBranch()
    {
        var (s, trigger, _) = CreateBaseStrategy();
        var cond = new BotNode(s.Id, BotNodeType.Condition, "Price == 60000",
            """{"indicator":"price","operator":"equal_to","value":60000}""", 200, 200);
        var actionTrue = new BotNode(s.Id, BotNodeType.Action, "Buy", """{"type":"buy"}""", 200, 400);
        var actionFalse = new BotNode(s.Id, BotNodeType.Action, "Sell", """{"type":"sell"}""", 400, 400);
        s.BotNodes.Add(cond); s.BotNodes.Add(actionTrue); s.BotNodes.Add(actionFalse);
        s.BotEdges.Clear();
        s.BotEdges.Add(new BotEdge(s.Id, trigger.Id, NodePort.Out, cond.Id));
        s.BotEdges.Add(new BotEdge(s.Id, cond.Id, NodePort.True, actionTrue.Id));
        s.BotEdges.Add(new BotEdge(s.Id, cond.Id, NodePort.False, actionFalse.Id));
        var runner = CreateRunner(s, out var executionRepo);

        await runner.ExecuteAsync(MakePriceEvent("BTCUSDT", 50000), trigger.Id, CancellationToken.None);

        var execution = Assert.Single(await executionRepo.GetAllExecutionsAsync());
        Assert.Equal(ExecutionStatus.Completed, execution.Status);
        Assert.Equal(actionFalse.Id, execution.CurrentNodeId);
    }

    [Fact]
    public async Task Condition_InvalidConfig_DefaultsToTrue()
    {
        var (s, trigger, _) = CreateBaseStrategy();
        var cond = new BotNode(s.Id, BotNodeType.Condition, "Bad config",
            """{"indicator":"price","operator":"nonexistent","value":40000}""", 200, 200);
        var actionTrue = new BotNode(s.Id, BotNodeType.Action, "Buy", """{"type":"buy"}""", 200, 400);
        s.BotNodes.Add(cond); s.BotNodes.Add(actionTrue);
        s.BotEdges.Clear();
        s.BotEdges.Add(new BotEdge(s.Id, trigger.Id, NodePort.Out, cond.Id));
        s.BotEdges.Add(new BotEdge(s.Id, cond.Id, NodePort.True, actionTrue.Id));
        var runner = CreateRunner(s, out var executionRepo);

        await runner.ExecuteAsync(MakePriceEvent("BTCUSDT", 50000), trigger.Id, CancellationToken.None);

        var execution = Assert.Single(await executionRepo.GetAllExecutionsAsync());
        Assert.Equal(ExecutionStatus.Completed, execution.Status);
    }

    [Fact]
    public async Task Condition_MissingOperator_DefaultsToGreaterThan()
    {
        var (s, trigger, _) = CreateBaseStrategy();
        var cond = new BotNode(s.Id, BotNodeType.Condition, "Price ? 40000",
            """{"indicator":"price","value":40000}""", 200, 200);
        var actionTrue = new BotNode(s.Id, BotNodeType.Action, "Buy", """{"type":"buy"}""", 200, 400);
        var actionFalse = new BotNode(s.Id, BotNodeType.Action, "Sell", """{"type":"sell"}""", 400, 400);
        s.BotNodes.Add(cond); s.BotNodes.Add(actionTrue); s.BotNodes.Add(actionFalse);
        s.BotEdges.Clear();
        s.BotEdges.Add(new BotEdge(s.Id, trigger.Id, NodePort.Out, cond.Id));
        s.BotEdges.Add(new BotEdge(s.Id, cond.Id, NodePort.True, actionTrue.Id));
        s.BotEdges.Add(new BotEdge(s.Id, cond.Id, NodePort.False, actionFalse.Id));
        var runner = CreateRunner(s, out var executionRepo);

        await runner.ExecuteAsync(MakePriceEvent("BTCUSDT", 50000), trigger.Id, CancellationToken.None);

        var execution = Assert.Single(await executionRepo.GetAllExecutionsAsync());
        Assert.Equal(ExecutionStatus.Completed, execution.Status);
        Assert.Equal(actionTrue.Id, execution.CurrentNodeId);
    }

    [Fact]
    public async Task Condition_CrossAbove_EvaluatesLikeGreaterThan()
    {
        var (s, trigger, _) = CreateBaseStrategy();
        var cond = new BotNode(s.Id, BotNodeType.Condition, "Cross above 40000",
            """{"indicator":"price","operator":"cross_above","value":40000}""", 200, 200);
        var actionTrue = new BotNode(s.Id, BotNodeType.Action, "Buy", """{"type":"buy"}""", 200, 400);
        s.BotNodes.Add(cond); s.BotNodes.Add(actionTrue);
        s.BotEdges.Clear();
        s.BotEdges.Add(new BotEdge(s.Id, trigger.Id, NodePort.Out, cond.Id));
        s.BotEdges.Add(new BotEdge(s.Id, cond.Id, NodePort.True, actionTrue.Id));
        var runner = CreateRunner(s, out var executionRepo);

        await runner.ExecuteAsync(MakePriceEvent("BTCUSDT", 50000), trigger.Id, CancellationToken.None);

        var execution = Assert.Single(await executionRepo.GetAllExecutionsAsync());
        Assert.Equal(ExecutionStatus.Completed, execution.Status);
    }

    [Fact]
    public async Task Action_Buy_DoesNotThrow_WhenServicesUnavailable()
    {
        var (s, trigger, action) = CreateBaseStrategy();
        var runner = CreateRunner(s, out var executionRepo);

        await runner.ExecuteAsync(MakePriceEvent("BTCUSDT", 50000), trigger.Id, CancellationToken.None);

        var execution = Assert.Single(await executionRepo.GetAllExecutionsAsync());
        Assert.Equal(ExecutionStatus.Completed, execution.Status);
    }

    [Fact]
    public async Task Condition_ConditionResultInFlag_SerializedCorrectly()
    {
        var (s, trigger, _) = CreateBaseStrategy();
        var cond = new BotNode(s.Id, BotNodeType.Condition, "Price > 40000",
            """{"indicator":"price","operator":"greater_than","value":40000}""", 200, 200);
        var act = new BotNode(s.Id, BotNodeType.Action, "Buy", """{"type":"buy"}""", 200, 400);
        s.BotNodes.Add(cond); s.BotNodes.Add(act);
        s.BotEdges.Clear();
        s.BotEdges.Add(new BotEdge(s.Id, trigger.Id, NodePort.Out, cond.Id));
        s.BotEdges.Add(new BotEdge(s.Id, cond.Id, NodePort.True, act.Id));
        var runner = CreateRunner(s, out var executionRepo);

        await runner.ExecuteAsync(MakePriceEvent("BTCUSDT", 50000), trigger.Id, CancellationToken.None);

        var execution = Assert.Single(await executionRepo.GetAllExecutionsAsync());
        Assert.NotNull(execution.CurrentFlag);
        Assert.Contains("condition_result", execution.CurrentFlag);
        Assert.Contains("true", execution.CurrentFlag, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Condition_GreaterOrEqualTrue_FollowsTrueBranch()
    {
        var (s, trigger, _) = CreateBaseStrategy();
        var cond = new BotNode(s.Id, BotNodeType.Condition, "Price >= 50000",
            """{"indicator":"price","operator":"greater_or_equal","value":50000}""", 200, 200);
        var actionTrue = new BotNode(s.Id, BotNodeType.Action, "Buy", """{"type":"buy"}""", 200, 400);
        var actionFalse = new BotNode(s.Id, BotNodeType.Action, "Sell", """{"type":"sell"}""", 400, 400);
        s.BotNodes.Add(cond); s.BotNodes.Add(actionTrue); s.BotNodes.Add(actionFalse);
        s.BotEdges.Clear();
        s.BotEdges.Add(new BotEdge(s.Id, trigger.Id, NodePort.Out, cond.Id));
        s.BotEdges.Add(new BotEdge(s.Id, cond.Id, NodePort.True, actionTrue.Id));
        s.BotEdges.Add(new BotEdge(s.Id, cond.Id, NodePort.False, actionFalse.Id));
        var runner = CreateRunner(s, out var executionRepo);

        await runner.ExecuteAsync(MakePriceEvent("BTCUSDT", 50000), trigger.Id, CancellationToken.None);

        var execution = Assert.Single(await executionRepo.GetAllExecutionsAsync());
        Assert.Equal(ExecutionStatus.Completed, execution.Status);
        Assert.Equal(actionTrue.Id, execution.CurrentNodeId);
    }

    [Fact]
    public async Task Condition_GreaterOrEqualFalse_FollowsFalseBranch()
    {
        var (s, trigger, _) = CreateBaseStrategy();
        var cond = new BotNode(s.Id, BotNodeType.Condition, "Price >= 60000",
            """{"indicator":"price","operator":"greater_or_equal","value":60000}""", 200, 200);
        var actionTrue = new BotNode(s.Id, BotNodeType.Action, "Buy", """{"type":"buy"}""", 200, 400);
        var actionFalse = new BotNode(s.Id, BotNodeType.Action, "Sell", """{"type":"sell"}""", 400, 400);
        s.BotNodes.Add(cond); s.BotNodes.Add(actionTrue); s.BotNodes.Add(actionFalse);
        s.BotEdges.Clear();
        s.BotEdges.Add(new BotEdge(s.Id, trigger.Id, NodePort.Out, cond.Id));
        s.BotEdges.Add(new BotEdge(s.Id, cond.Id, NodePort.True, actionTrue.Id));
        s.BotEdges.Add(new BotEdge(s.Id, cond.Id, NodePort.False, actionFalse.Id));
        var runner = CreateRunner(s, out var executionRepo);

        await runner.ExecuteAsync(MakePriceEvent("BTCUSDT", 50000), trigger.Id, CancellationToken.None);

        var execution = Assert.Single(await executionRepo.GetAllExecutionsAsync());
        Assert.Equal(ExecutionStatus.Completed, execution.Status);
        Assert.Equal(actionFalse.Id, execution.CurrentNodeId);
    }

    [Fact]
    public async Task Condition_LessOrEqualTrue_FollowsTrueBranch()
    {
        var (s, trigger, _) = CreateBaseStrategy();
        var cond = new BotNode(s.Id, BotNodeType.Condition, "Price <= 50000",
            """{"indicator":"price","operator":"less_or_equal","value":50000}""", 200, 200);
        var actionTrue = new BotNode(s.Id, BotNodeType.Action, "Buy", """{"type":"buy"}""", 200, 400);
        var actionFalse = new BotNode(s.Id, BotNodeType.Action, "Sell", """{"type":"sell"}""", 400, 400);
        s.BotNodes.Add(cond); s.BotNodes.Add(actionTrue); s.BotNodes.Add(actionFalse);
        s.BotEdges.Clear();
        s.BotEdges.Add(new BotEdge(s.Id, trigger.Id, NodePort.Out, cond.Id));
        s.BotEdges.Add(new BotEdge(s.Id, cond.Id, NodePort.True, actionTrue.Id));
        s.BotEdges.Add(new BotEdge(s.Id, cond.Id, NodePort.False, actionFalse.Id));
        var runner = CreateRunner(s, out var executionRepo);

        await runner.ExecuteAsync(MakePriceEvent("BTCUSDT", 50000), trigger.Id, CancellationToken.None);

        var execution = Assert.Single(await executionRepo.GetAllExecutionsAsync());
        Assert.Equal(ExecutionStatus.Completed, execution.Status);
        Assert.Equal(actionTrue.Id, execution.CurrentNodeId);
    }

    [Fact]
    public async Task Condition_LessOrEqualFalse_FollowsFalseBranch()
    {
        var (s, trigger, _) = CreateBaseStrategy();
        var cond = new BotNode(s.Id, BotNodeType.Condition, "Price <= 40000",
            """{"indicator":"price","operator":"less_or_equal","value":40000}""", 200, 200);
        var actionTrue = new BotNode(s.Id, BotNodeType.Action, "Buy", """{"type":"buy"}""", 200, 400);
        var actionFalse = new BotNode(s.Id, BotNodeType.Action, "Sell", """{"type":"sell"}""", 400, 400);
        s.BotNodes.Add(cond); s.BotNodes.Add(actionTrue); s.BotNodes.Add(actionFalse);
        s.BotEdges.Clear();
        s.BotEdges.Add(new BotEdge(s.Id, trigger.Id, NodePort.Out, cond.Id));
        s.BotEdges.Add(new BotEdge(s.Id, cond.Id, NodePort.True, actionTrue.Id));
        s.BotEdges.Add(new BotEdge(s.Id, cond.Id, NodePort.False, actionFalse.Id));
        var runner = CreateRunner(s, out var executionRepo);

        await runner.ExecuteAsync(MakePriceEvent("BTCUSDT", 50000), trigger.Id, CancellationToken.None);

        var execution = Assert.Single(await executionRepo.GetAllExecutionsAsync());
        Assert.Equal(ExecutionStatus.Completed, execution.Status);
        Assert.Equal(actionFalse.Id, execution.CurrentNodeId);
    }

    [Fact]
    public async Task Condition_CrossBelow_EvaluatesLikeLessThan()
    {
        var (s, trigger, _) = CreateBaseStrategy();
        var cond = new BotNode(s.Id, BotNodeType.Condition, "Cross below 60000",
            """{"indicator":"price","operator":"cross_below","value":60000}""", 200, 200);
        var actionTrue = new BotNode(s.Id, BotNodeType.Action, "Buy", """{"type":"buy"}""", 200, 400);
        s.BotNodes.Add(cond); s.BotNodes.Add(actionTrue);
        s.BotEdges.Clear();
        s.BotEdges.Add(new BotEdge(s.Id, trigger.Id, NodePort.Out, cond.Id));
        s.BotEdges.Add(new BotEdge(s.Id, cond.Id, NodePort.True, actionTrue.Id));
        var runner = CreateRunner(s, out var executionRepo);

        await runner.ExecuteAsync(MakePriceEvent("BTCUSDT", 50000), trigger.Id, CancellationToken.None);

        var execution = Assert.Single(await executionRepo.GetAllExecutionsAsync());
        Assert.Equal(ExecutionStatus.Completed, execution.Status);
    }

    [Fact]
    public async Task Condition_InvalidJson_DefaultsToTrue()
    {
        var (s, trigger, _) = CreateBaseStrategy();
        var cond = new BotNode(s.Id, BotNodeType.Condition, "Bad JSON",
            """not valid json""", 200, 200);
        var actionTrue = new BotNode(s.Id, BotNodeType.Action, "Buy", """{"type":"buy"}""", 200, 400);
        s.BotNodes.Add(cond); s.BotNodes.Add(actionTrue);
        s.BotEdges.Clear();
        s.BotEdges.Add(new BotEdge(s.Id, trigger.Id, NodePort.Out, cond.Id));
        s.BotEdges.Add(new BotEdge(s.Id, cond.Id, NodePort.True, actionTrue.Id));
        var runner = CreateRunner(s, out var executionRepo);

        await runner.ExecuteAsync(MakePriceEvent("BTCUSDT", 50000), trigger.Id, CancellationToken.None);

        var execution = Assert.Single(await executionRepo.GetAllExecutionsAsync());
        Assert.Equal(ExecutionStatus.Completed, execution.Status);
    }

    [Fact]
    public async Task Action_BuyWithQuantity_PastValidation_CaughtByHandler()
    {
        var (s, trigger, _) = CreateBaseStrategy();
        var actionBuy = new BotNode(s.Id, BotNodeType.Action, "BuyWithQty",
            """{"type":"buy","quantity":0.001}""", 100, 300);
        s.BotNodes.Add(actionBuy);
        s.BotEdges.Clear();
        s.BotEdges.Add(new BotEdge(s.Id, trigger.Id, NodePort.Out, actionBuy.Id));
        var runner = CreateRunner(s, out var executionRepo);

        await runner.ExecuteAsync(MakePriceEvent("BTCUSDT", 50000), trigger.Id, CancellationToken.None);

        var execution = Assert.Single(await executionRepo.GetAllExecutionsAsync());
        Assert.Equal(ExecutionStatus.Completed, execution.Status);
    }

    [Fact]
    public async Task Action_SellWithQuantity_PastValidation_CaughtByHandler()
    {
        var (s, trigger, _) = CreateBaseStrategy();
        var actionSell = new BotNode(s.Id, BotNodeType.Action, "SellWithQty",
            """{"type":"sell","quantity":0.001}""", 100, 300);
        s.BotNodes.Add(actionSell);
        s.BotEdges.Clear();
        s.BotEdges.Add(new BotEdge(s.Id, trigger.Id, NodePort.Out, actionSell.Id));
        var runner = CreateRunner(s, out var executionRepo);

        await runner.ExecuteAsync(MakePriceEvent("BTCUSDT", 50000), trigger.Id, CancellationToken.None);

        var execution = Assert.Single(await executionRepo.GetAllExecutionsAsync());
        Assert.Equal(ExecutionStatus.Completed, execution.Status);
    }

    // ---- Helpers ----

    private static (Strategy, BotNode, BotNode) CreateBaseStrategy()
    {
        var s = new Strategy(Guid.NewGuid(), "Test", Guid.NewGuid());
        s.StartEngine();
        var t = new BotNode(s.Id, BotNodeType.Trigger, "Trigger", """{"symbol": "BTCUSDT"}""", 100, 100);
        var a = new BotNode(s.Id, BotNodeType.Action, "Notify", """{"type":"notify"}""", 100, 300);
        s.BotNodes.Add(t); s.BotNodes.Add(a);
        s.BotEdges.Add(new BotEdge(s.Id, t.Id, NodePort.Out, a.Id));
        return (s, t, a);
    }

    private static (Strategy, BotNode, BotNode, BotNode) CreateTwoTriggerStrategy()
    {
        var s = new Strategy(Guid.NewGuid(), "Multi", Guid.NewGuid());
        s.StartEngine();
        var btc = new BotNode(s.Id, BotNodeType.Trigger, "BTC", """{"symbol":"BTCUSDT"}""", 100, 100);
        var eth = new BotNode(s.Id, BotNodeType.Trigger, "ETH", """{"symbol":"ETHUSDT"}""", 300, 100);
        var a = new BotNode(s.Id, BotNodeType.Action, "Notify", """{"type":"notify"}""", 200, 300);
        s.BotNodes.Add(btc); s.BotNodes.Add(eth); s.BotNodes.Add(a);
        s.BotEdges.Add(new BotEdge(s.Id, btc.Id, NodePort.Out, a.Id));
        s.BotEdges.Add(new BotEdge(s.Id, eth.Id, NodePort.Out, a.Id));
        return (s, btc, eth, a);
    }

    private static BotGraphRunner CreateRunner(Strategy s, out MockExecutionRepo repo)
    {
        repo = new MockExecutionRepo();
        var sp = new ServiceCollection()
            .AddSingleton<IStrategyExecutionRepository>(repo)
            .BuildServiceProvider();
        return new BotGraphRunner(s, sp.GetRequiredService<IServiceScopeFactory>());
    }

    private static IStrategyEngine BuildEngine(Strategy? s)
    {
        var sp = new ServiceCollection()
            .AddSingleton<IStrategyRepository>(new MockStrategyRepository(s))
            .AddSingleton<IStrategyExecutionRepository>(new MockExecutionRepo())
            .AddSingleton<IMarketDataEventBus>(new MarketDataEventBus())
            .AddSingleton<StrategyEngineService>()
            .AddSingleton<IStrategyEngine>(sp => sp.GetRequiredService<StrategyEngineService>())
            .BuildServiceProvider();
        return sp.GetRequiredService<IStrategyEngine>();
    }

    private static MarketPriceEvent MakePriceEvent(string symbol, decimal price) =>
        new(symbol, price, DateTime.UtcNow);
}

public class MockStrategyRepository : IStrategyRepository
{
    private readonly Strategy? _s;
    public MockStrategyRepository(Strategy? s) => _s = s;
    public Task<Strategy?> GetByIdAsync(Guid id) => Task.FromResult(_s?.Id == id ? _s : null);
    public Task<Strategy?> GetByIdWithGraphAsync(Guid id) => GetByIdAsync(id);
    public Task<List<Strategy>> GetByTraderIdAsync(string _) =>
        Task.FromResult(_s != null ? new List<Strategy> { _s } : new List<Strategy>());
    public Task<List<Strategy>> GetActiveWithEngineRunningAsync() =>
        Task.FromResult(_s != null ? new List<Strategy> { _s } : new List<Strategy>());
    public Task AddAsync(Strategy s) => Task.CompletedTask;
    public Task SaveChangesAsync() => Task.CompletedTask;
}

public class MockExecutionRepo : IStrategyExecutionRepository
{
    private readonly List<StrategyExecution> _list = new();
    public Task<StrategyExecution?> GetActiveByStrategyIdAsync(Guid id) =>
        Task.FromResult(_list.FirstOrDefault(e => e.StrategyId == id && e.Status == ExecutionStatus.Running));
    public Task<List<StrategyExecution>> GetAllExecutionsAsync() => Task.FromResult(_list.ToList());
    public Task AddAsync(StrategyExecution e) { _list.Add(e); return Task.CompletedTask; }
    public void Update(StrategyExecution e) { }
    public Task SaveChangesAsync() => Task.CompletedTask;
}
