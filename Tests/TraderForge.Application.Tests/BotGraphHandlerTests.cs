using Moq;
using TraderForge.Domain.Common;
using TraderForge.Application.DTOs;
using TraderForge.Application.Handlers;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Enums;
using TraderForge.Domain.Repositories;
using TraderForge.Domain.Services;

namespace TraderForge.Application.Tests;

public class AddBotNodeCommandHandlerTests
{
    private readonly Mock<IBotNodeRepository> _repo;
    private readonly AddBotNodeCommandHandler _handler;

    public AddBotNodeCommandHandlerTests()
    {
        _repo = new Mock<IBotNodeRepository>();
        _handler = new AddBotNodeCommandHandler(_repo.Object);
    }

    [Fact]
    public async Task HandleAsync_Valid_ReturnsNodeId()
    {
        var cmd = new AddBotNodeCommand
        {
            StrategyId = Guid.NewGuid(),
            Type = BotNodeType.Trigger,
            Name = "Test Trigger",
            Config = """{"symbol":"BTCUSDT"}""",
            PositionX = 100, PositionY = 200
        };

        var result = await _handler.HandleAsync(cmd);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
        _repo.Verify(r => r.AddAsync(It.IsAny<BotNode>()), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Exception_ReturnsFailure()
    {
        _repo.Setup(r => r.AddAsync(It.IsAny<BotNode>())).ThrowsAsync(new Exception("DB error"));
        var cmd = new AddBotNodeCommand { StrategyId = Guid.NewGuid(), Type = BotNodeType.Action, Name = "X", Config = "{}" };

        var result = await _handler.HandleAsync(cmd);

        Assert.False(result.IsSuccess);
        Assert.Equal("DB error", result.ErrorMessage);
    }
}

public class UpdateBotNodeCommandHandlerTests
{
    private readonly Mock<IBotNodeRepository> _repo;
    private readonly UpdateBotNodeCommandHandler _handler;

    public UpdateBotNodeCommandHandlerTests()
    {
        _repo = new Mock<IBotNodeRepository>();
        _handler = new UpdateBotNodeCommandHandler(_repo.Object);
    }

    [Fact]
    public async Task HandleAsync_NodeFound_UpdatesAndSaves()
    {
        var node = new BotNode(Guid.NewGuid(), BotNodeType.Trigger, "Old", """{"symbol":"BTC"}""", 0, 0);
        _repo.Setup(r => r.GetByIdAsync(node.Id)).ReturnsAsync(node);
        var cmd = new UpdateBotNodeCommand { Id = node.Id, Name = "New", Config = """{"symbol":"ETH"}""", PositionX = 50, PositionY = 100 };

        var result = await _handler.HandleAsync(cmd);

        Assert.True(result.IsSuccess);
        _repo.Verify(r => r.Update(node), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_NodeNotFound_ReturnsFailure()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((BotNode?)null);
        var cmd = new UpdateBotNodeCommand { Id = Guid.NewGuid(), Name = "X", Config = "{}" };

        var result = await _handler.HandleAsync(cmd);

        Assert.False(result.IsSuccess);
        Assert.Equal("BotNode not found.", result.ErrorMessage);
    }
}

public class RemoveBotNodeCommandHandlerTests
{
    private readonly Mock<IBotNodeRepository> _repo;
    private readonly RemoveBotNodeCommandHandler _handler;

    public RemoveBotNodeCommandHandlerTests()
    {
        _repo = new Mock<IBotNodeRepository>();
        _handler = new RemoveBotNodeCommandHandler(_repo.Object);
    }

    [Fact]
    public async Task HandleAsync_NodeFound_RemovesAndSaves()
    {
        var node = new BotNode(Guid.NewGuid(), BotNodeType.Trigger, "T", "{}", 0, 0);
        _repo.Setup(r => r.GetByIdAsync(node.Id)).ReturnsAsync(node);

        var result = await _handler.HandleAsync(new RemoveBotNodeCommand { Id = node.Id });

        Assert.True(result.IsSuccess);
        _repo.Verify(r => r.Remove(node), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_NodeNotFound_ReturnsFailure()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((BotNode?)null);

        var result = await _handler.HandleAsync(new RemoveBotNodeCommand { Id = Guid.NewGuid() });

        Assert.False(result.IsSuccess);
    }
}

public class AddBotEdgeCommandHandlerTests
{
    private readonly Mock<IBotEdgeRepository> _edgeRepo;
    private readonly Mock<IBotNodeRepository> _nodeRepo;
    private readonly AddBotEdgeCommandHandler _handler;

    public AddBotEdgeCommandHandlerTests()
    {
        _edgeRepo = new Mock<IBotEdgeRepository>();
        _nodeRepo = new Mock<IBotNodeRepository>();
        _handler = new AddBotEdgeCommandHandler(_edgeRepo.Object, _nodeRepo.Object);
    }

    [Fact]
    public async Task HandleAsync_Valid_ReturnsEdgeId()
    {
        var sid = Guid.NewGuid();
        var src = new BotNode(sid, BotNodeType.Trigger, "Src", "{}", 0, 0);
        var tgt = new BotNode(sid, BotNodeType.Action, "Tgt", "{}", 0, 0);
        _nodeRepo.Setup(r => r.GetByIdAsync(src.Id)).ReturnsAsync(src);
        _nodeRepo.Setup(r => r.GetByIdAsync(tgt.Id)).ReturnsAsync(tgt);

        var cmd = new AddBotEdgeCommand { StrategyId = sid, SourceNodeId = src.Id, SourcePort = NodePort.Out, TargetNodeId = tgt.Id };
        var result = await _handler.HandleAsync(cmd);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
        _edgeRepo.Verify(r => r.AddAsync(It.IsAny<BotEdge>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_SourceNotFound_ReturnsFailure()
    {
        _nodeRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((BotNode?)null);

        var result = await _handler.HandleAsync(new AddBotEdgeCommand { StrategyId = Guid.NewGuid(), SourceNodeId = Guid.NewGuid(), TargetNodeId = Guid.NewGuid() });

        Assert.False(result.IsSuccess);
        Assert.Contains("Source node", result.ErrorMessage);
    }

    [Fact]
    public async Task HandleAsync_TargetNotFound_ReturnsFailure()
    {
        var src = new BotNode(Guid.NewGuid(), BotNodeType.Trigger, "Src", "{}", 0, 0);
        _nodeRepo.Setup(r => r.GetByIdAsync(src.Id)).ReturnsAsync(src);
        _nodeRepo.Setup(r => r.GetByIdAsync(It.Is<Guid>(x => x != src.Id))).ReturnsAsync((BotNode?)null);

        var result = await _handler.HandleAsync(new AddBotEdgeCommand { StrategyId = Guid.NewGuid(), SourceNodeId = src.Id, TargetNodeId = Guid.NewGuid() });

        Assert.False(result.IsSuccess);
        Assert.Contains("Target node", result.ErrorMessage);
    }

    [Fact]
    public async Task HandleAsync_DifferentStrategies_ReturnsFailure()
    {
        var src = new BotNode(Guid.NewGuid(), BotNodeType.Trigger, "Src", "{}", 0, 0);
        var tgt = new BotNode(Guid.NewGuid(), BotNodeType.Action, "Tgt", "{}", 0, 0);
        _nodeRepo.Setup(r => r.GetByIdAsync(src.Id)).ReturnsAsync(src);
        _nodeRepo.Setup(r => r.GetByIdAsync(tgt.Id)).ReturnsAsync(tgt);

        var result = await _handler.HandleAsync(new AddBotEdgeCommand { StrategyId = Guid.NewGuid(), SourceNodeId = src.Id, TargetNodeId = tgt.Id });

        Assert.False(result.IsSuccess);
    }
}

public class RemoveBotEdgeCommandHandlerTests
{
    private readonly Mock<IBotEdgeRepository> _repo;
    private readonly RemoveBotEdgeCommandHandler _handler;

    public RemoveBotEdgeCommandHandlerTests()
    {
        _repo = new Mock<IBotEdgeRepository>();
        _handler = new RemoveBotEdgeCommandHandler(_repo.Object);
    }

    [Fact]
    public async Task HandleAsync_EdgeFound_Removes()
    {
        var e = new BotEdge(Guid.NewGuid(), Guid.NewGuid(), NodePort.Out, Guid.NewGuid());
        _repo.Setup(r => r.GetByIdAsync(e.Id)).ReturnsAsync(e);

        var result = await _handler.HandleAsync(new RemoveBotEdgeCommand { Id = e.Id });

        Assert.True(result.IsSuccess);
        _repo.Verify(r => r.Remove(e), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_EdgeNotFound_ReturnsFailure()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((BotEdge?)null);

        var result = await _handler.HandleAsync(new RemoveBotEdgeCommand { Id = Guid.NewGuid() });

        Assert.False(result.IsSuccess);
    }
}

public class StartEngineCommandHandlerTests
{
    private readonly Mock<IStrategyRepository> _strategyRepo;
    private readonly Mock<IStrategyEngine> _engine;
    private readonly StartEngineCommandHandler _handler;

    public StartEngineCommandHandlerTests()
    {
        _strategyRepo = new Mock<IStrategyRepository>();
        _engine = new Mock<IStrategyEngine>();
        _handler = new StartEngineCommandHandler(_strategyRepo.Object, _engine.Object);
    }

    [Fact]
    public async Task HandleAsync_ValidWithTrigger_StartsEngine()
    {
        var s = new Strategy(Guid.NewGuid(), "Test", Guid.NewGuid());
        s.StartEngine();
        var t = new BotNode(s.Id, BotNodeType.Trigger, "T", """{"symbol":"BTC"}""", 0, 0);
        s.BotNodes.Add(t);
        _strategyRepo.Setup(r => r.GetByIdWithGraphAsync(s.Id)).ReturnsAsync(s);

        var result = await _handler.HandleAsync(new StartEngineCommand { StrategyId = s.Id });

        Assert.True(result.IsSuccess);
        _engine.Verify(e => e.StartStrategyAsync(s.Id), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_StrategyNotFound_ReturnsFailure()
    {
        _strategyRepo.Setup(r => r.GetByIdWithGraphAsync(It.IsAny<Guid>())).ReturnsAsync((Strategy?)null);

        var result = await _handler.HandleAsync(new StartEngineCommand { StrategyId = Guid.NewGuid() });

        Assert.False(result.IsSuccess);
        Assert.Contains("not found", result.ErrorMessage);
    }

    [Fact]
    public async Task HandleAsync_StrategyNotActive_ReturnsFailure()
    {
        var s = new Strategy(Guid.NewGuid(), "Test", Guid.NewGuid());
        s.Deactivate();
        _strategyRepo.Setup(r => r.GetByIdWithGraphAsync(s.Id)).ReturnsAsync(s);

        var result = await _handler.HandleAsync(new StartEngineCommand { StrategyId = s.Id });

        Assert.False(result.IsSuccess);
        Assert.Contains("not active", result.ErrorMessage);
    }

    [Fact]
    public async Task HandleAsync_NoTriggerNode_ReturnsFailure()
    {
        var s = new Strategy(Guid.NewGuid(), "Test", Guid.NewGuid());
        s.StartEngine();
        _strategyRepo.Setup(r => r.GetByIdWithGraphAsync(s.Id)).ReturnsAsync(s);

        var result = await _handler.HandleAsync(new StartEngineCommand { StrategyId = s.Id });

        Assert.False(result.IsSuccess);
        Assert.Contains("Trigger", result.ErrorMessage);
    }
}

public class StopEngineCommandHandlerTests
{
    private readonly Mock<IStrategyEngine> _engine;
    private readonly StopEngineCommandHandler _handler;

    public StopEngineCommandHandlerTests()
    {
        _engine = new Mock<IStrategyEngine>();
        _handler = new StopEngineCommandHandler(_engine.Object);
    }

    [Fact]
    public async Task HandleAsync_StopsEngine()
    {
        var id = Guid.NewGuid();
        var result = await _handler.HandleAsync(new StopEngineCommand { StrategyId = id });

        Assert.True(result.IsSuccess);
        _engine.Verify(e => e.StopStrategyAsync(id), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Exception_ReturnsFailure()
    {
        _engine.Setup(e => e.StopStrategyAsync(It.IsAny<Guid>())).ThrowsAsync(new Exception("fail"));

        var result = await _handler.HandleAsync(new StopEngineCommand { StrategyId = Guid.NewGuid() });

        Assert.False(result.IsSuccess);
    }
}
