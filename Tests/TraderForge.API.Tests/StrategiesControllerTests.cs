using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TraderForge.API.Controllers;
using TraderForge.API.Mappers;
using TraderForge.API.Requests;
using TraderForge.Application.DTOs;
using TraderForge.Application.Handlers;
using TraderForge.Domain.Common;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Enums;
using TraderForge.Domain.Repositories;
using TraderForge.Domain.Services;

namespace TraderForge.API.Tests;

public class StrategiesControllerTests
{
    private readonly Mock<IStrategyRepository> _strategyRepo;
    private readonly Mock<IBotNodeRepository> _nodeRepo;
    private readonly Mock<IBotEdgeRepository> _edgeRepo;
    private readonly Mock<IStrategyEngine> _engine;
    private readonly StrategiesController _controller;
    private readonly string _traderId = "test-trader-id";

    public StrategiesControllerTests()
    {
        _strategyRepo = new Mock<IStrategyRepository>();
        _nodeRepo = new Mock<IBotNodeRepository>();
        _edgeRepo = new Mock<IBotEdgeRepository>();
        _engine = new Mock<IStrategyEngine>();

        _strategyRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Strategy?)null);

        var addNodeHandler = new AddBotNodeCommandHandler(_nodeRepo.Object);
        var updateNodeHandler = new UpdateBotNodeCommandHandler(_nodeRepo.Object);
        var removeNodeHandler = new RemoveBotNodeCommandHandler(_nodeRepo.Object);
        var addEdgeHandler = new AddBotEdgeCommandHandler(_edgeRepo.Object, _nodeRepo.Object);
        var removeEdgeHandler = new RemoveBotEdgeCommandHandler(_edgeRepo.Object);
        var startEngineHandler = new StartEngineCommandHandler(_strategyRepo.Object, _engine.Object);
        var stopEngineHandler = new StopEngineCommandHandler(_engine.Object);
        var updateStrategyStateHandler = new UpdateStrategyStateCommandHandler(_strategyRepo.Object);

        _controller = new StrategiesController(
            _strategyRepo.Object,
            _nodeRepo.Object,
            _edgeRepo.Object,
            addNodeHandler,
            updateNodeHandler,
            removeNodeHandler,
            addEdgeHandler,
            removeEdgeHandler,
            startEngineHandler,
            stopEngineHandler,
            updateStrategyStateHandler);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, _traderId),
        }, "test"));
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    private static Strategy CreateStrategyWithPortfolio(Guid id, string traderId)
    {
        var portfolio = new Portfolio(traderId, 10000m);
        var strategy = new Strategy(id, "Test Strategy", portfolio.Id);
        var prop = typeof(Strategy).GetProperty(nameof(Strategy.Portfolio))!;
        prop.GetSetMethod(nonPublic: true)!.Invoke(strategy, [portfolio]);
        return strategy;
    }

    [Fact]
    public async Task GetGraph_StrategyNotFound_ReturnsNotFound()
    {
        _strategyRepo.Setup(r => r.GetByIdWithGraphAsync(It.IsAny<Guid>())).ReturnsAsync((Strategy?)null);

        var result = await _controller.GetGraph(Guid.NewGuid());

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetGraph_NotOwner_ReturnsForbid()
    {
        var strategy = CreateStrategyWithPortfolio(Guid.NewGuid(), "other-trader");
        _strategyRepo.Setup(r => r.GetByIdWithGraphAsync(strategy.Id)).ReturnsAsync(strategy);

        var result = await _controller.GetGraph(strategy.Id);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task GetGraph_ReturnsOkWithGraphData()
    {
        var strategyId = Guid.NewGuid();
        var strategy = CreateStrategyWithPortfolio(strategyId, _traderId);
        var node = new BotNode(strategyId, BotNodeType.Trigger, "T1", """{"symbol":"BTC"}""", 100, 200);
        strategy.BotNodes.Add(node);
        strategy.BotEdges.Add(new BotEdge(strategyId, Guid.NewGuid(), NodePort.Out, Guid.NewGuid()));
        _strategyRepo.Setup(r => r.GetByIdWithGraphAsync(strategyId)).ReturnsAsync(strategy);

        var result = await _controller.GetGraph(strategyId);

        var ok = Assert.IsType<OkObjectResult>(result);
        var value = ok.Value;
        var dict = value.GetType();
        Assert.Equal(strategyId, dict.GetProperty("Id")!.GetValue(value));
        Assert.Equal("Test Strategy", dict.GetProperty("Name")!.GetValue(value));
    }

    [Fact]
    public async Task AddNode_NotOwner_ReturnsForbid()
    {
        var result = await _controller.AddNode(Guid.NewGuid(), new AddBotNodeRequest());
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task AddNode_HandlerSuccess_ReturnsOkWithNodeId()
    {
        var strategy = CreateStrategyWithPortfolio(Guid.NewGuid(), _traderId);
        _strategyRepo.Setup(r => r.GetByIdAsync(strategy.Id)).ReturnsAsync(strategy);

        var result = await _controller.AddNode(strategy.Id, new AddBotNodeRequest
        {
            Type = BotNodeType.Trigger,
            Name = "Test",
            Config = "{}",
            PositionX = 10,
            PositionY = 20
        });

        var ok = Assert.IsType<OkObjectResult>(result);
        var nodeId = ok.Value.GetType().GetProperty("nodeId")!.GetValue(ok.Value);
        Assert.NotEqual(Guid.Empty, nodeId);
    }

    [Fact]
    public async Task AddNode_HandlerFails_ReturnsBadRequest()
    {
        var strategy = CreateStrategyWithPortfolio(Guid.NewGuid(), _traderId);
        _strategyRepo.Setup(r => r.GetByIdAsync(strategy.Id)).ReturnsAsync(strategy);
        _nodeRepo.Setup(r => r.AddAsync(It.IsAny<BotNode>())).ThrowsAsync(new Exception("fail"));

        var result = await _controller.AddNode(strategy.Id, new AddBotNodeRequest
        {
            Type = BotNodeType.Trigger,
            Name = "Test",
            Config = "{}",
            PositionX = 10,
            PositionY = 20
        });

        var bad = Assert.IsType<BadRequestObjectResult>(result);
        var error = bad.Value.GetType().GetProperty("error")!.GetValue(bad.Value);
        Assert.Equal("fail", error);
    }

    [Fact]
    public async Task UpdateNode_NotOwner_ReturnsForbid()
    {
        var result = await _controller.UpdateNode(Guid.NewGuid(), Guid.NewGuid(), new UpdateBotNodeRequest());
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task UpdateNode_HandlerSuccess_ReturnsOk()
    {
        var strategy = CreateStrategyWithPortfolio(Guid.NewGuid(), _traderId);
        _strategyRepo.Setup(r => r.GetByIdAsync(strategy.Id)).ReturnsAsync(strategy);
        var node = new BotNode(strategy.Id, BotNodeType.Condition, "Old", "{}", 0, 0);
        _nodeRepo.Setup(r => r.GetByIdAsync(node.Id)).ReturnsAsync(node);

        var result = await _controller.UpdateNode(strategy.Id, node.Id, new UpdateBotNodeRequest
        {
            Name = "New",
            Config = """{"val":1}""",
            PositionX = 50,
            PositionY = 100
        });

        var ok = Assert.IsType<OkObjectResult>(result);
        var msg = ok.Value.GetType().GetProperty("message")!.GetValue(ok.Value);
        Assert.Equal("Node updated.", msg);
    }

    [Fact]
    public async Task UpdateNode_HandlerFails_ReturnsBadRequest()
    {
        var strategy = CreateStrategyWithPortfolio(Guid.NewGuid(), _traderId);
        _strategyRepo.Setup(r => r.GetByIdAsync(strategy.Id)).ReturnsAsync(strategy);
        _nodeRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((BotNode?)null);

        var result = await _controller.UpdateNode(strategy.Id, Guid.NewGuid(), new UpdateBotNodeRequest());

        var bad = Assert.IsType<BadRequestObjectResult>(result);
        var error = bad.Value.GetType().GetProperty("error")!.GetValue(bad.Value);
        Assert.Equal("BotNode not found.", error);
    }

    [Fact]
    public async Task RemoveNode_NotOwner_ReturnsForbid()
    {
        var result = await _controller.RemoveNode(Guid.NewGuid(), Guid.NewGuid());
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task RemoveNode_HandlerSuccess_ReturnsOk()
    {
        var strategy = CreateStrategyWithPortfolio(Guid.NewGuid(), _traderId);
        _strategyRepo.Setup(r => r.GetByIdAsync(strategy.Id)).ReturnsAsync(strategy);
        var node = new BotNode(strategy.Id, BotNodeType.Action, "A", "{}", 0, 0);
        _nodeRepo.Setup(r => r.GetByIdAsync(node.Id)).ReturnsAsync(node);

        var result = await _controller.RemoveNode(strategy.Id, node.Id);

        var ok = Assert.IsType<OkObjectResult>(result);
        var msg = ok.Value.GetType().GetProperty("message")!.GetValue(ok.Value);
        Assert.Equal("Node removed.", msg);
    }

    [Fact]
    public async Task RemoveNode_HandlerFails_ReturnsBadRequest()
    {
        var strategy = CreateStrategyWithPortfolio(Guid.NewGuid(), _traderId);
        _strategyRepo.Setup(r => r.GetByIdAsync(strategy.Id)).ReturnsAsync(strategy);
        _nodeRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((BotNode?)null);

        var result = await _controller.RemoveNode(strategy.Id, Guid.NewGuid());

        var bad = Assert.IsType<BadRequestObjectResult>(result);
        var error = bad.Value.GetType().GetProperty("error")!.GetValue(bad.Value);
        Assert.Equal("BotNode not found.", error);
    }

    [Fact]
    public async Task AddEdge_NotOwner_ReturnsForbid()
    {
        var result = await _controller.AddEdge(Guid.NewGuid(), new AddBotEdgeRequest());
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task AddEdge_HandlerSuccess_ReturnsOkWithEdgeId()
    {
        var strategy = CreateStrategyWithPortfolio(Guid.NewGuid(), _traderId);
        _strategyRepo.Setup(r => r.GetByIdAsync(strategy.Id)).ReturnsAsync(strategy);
        var src = new BotNode(strategy.Id, BotNodeType.Trigger, "Src", "{}", 0, 0);
        var tgt = new BotNode(strategy.Id, BotNodeType.Action, "Tgt", "{}", 0, 0);
        _nodeRepo.Setup(r => r.GetByIdAsync(src.Id)).ReturnsAsync(src);
        _nodeRepo.Setup(r => r.GetByIdAsync(tgt.Id)).ReturnsAsync(tgt);

        var result = await _controller.AddEdge(strategy.Id, new AddBotEdgeRequest
        {
            SourceNodeId = src.Id,
            SourcePort = NodePort.Out,
            TargetNodeId = tgt.Id
        });

        var ok = Assert.IsType<OkObjectResult>(result);
        var edgeId = ok.Value.GetType().GetProperty("edgeId")!.GetValue(ok.Value);
        Assert.NotEqual(Guid.Empty, edgeId);
    }

    [Fact]
    public async Task AddEdge_HandlerFails_ReturnsBadRequest()
    {
        var strategy = CreateStrategyWithPortfolio(Guid.NewGuid(), _traderId);
        _strategyRepo.Setup(r => r.GetByIdAsync(strategy.Id)).ReturnsAsync(strategy);
        _nodeRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((BotNode?)null);

        var result = await _controller.AddEdge(strategy.Id, new AddBotEdgeRequest
        {
            SourceNodeId = Guid.NewGuid(),
            TargetNodeId = Guid.NewGuid()
        });

        var bad = Assert.IsType<BadRequestObjectResult>(result);
        var error = bad.Value.GetType().GetProperty("error")!.GetValue(bad.Value);
        Assert.Contains("Source node", ((string)error!));
    }

    [Fact]
    public async Task RemoveEdge_NotOwner_ReturnsForbid()
    {
        var result = await _controller.RemoveEdge(Guid.NewGuid(), Guid.NewGuid());
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task RemoveEdge_HandlerSuccess_ReturnsOk()
    {
        var strategy = CreateStrategyWithPortfolio(Guid.NewGuid(), _traderId);
        _strategyRepo.Setup(r => r.GetByIdAsync(strategy.Id)).ReturnsAsync(strategy);
        var edge = new BotEdge(strategy.Id, Guid.NewGuid(), NodePort.Out, Guid.NewGuid());
        _edgeRepo.Setup(r => r.GetByIdAsync(edge.Id)).ReturnsAsync(edge);

        var result = await _controller.RemoveEdge(strategy.Id, edge.Id);

        var ok = Assert.IsType<OkObjectResult>(result);
        var msg = ok.Value.GetType().GetProperty("message")!.GetValue(ok.Value);
        Assert.Equal("Edge removed.", msg);
    }

    [Fact]
    public async Task RemoveEdge_HandlerFails_ReturnsBadRequest()
    {
        var strategy = CreateStrategyWithPortfolio(Guid.NewGuid(), _traderId);
        _strategyRepo.Setup(r => r.GetByIdAsync(strategy.Id)).ReturnsAsync(strategy);
        _edgeRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((BotEdge?)null);

        var result = await _controller.RemoveEdge(strategy.Id, Guid.NewGuid());

        var bad = Assert.IsType<BadRequestObjectResult>(result);
        var error = bad.Value.GetType().GetProperty("error")!.GetValue(bad.Value);
        Assert.Equal("BotEdge not found.", error);
    }

    [Fact]
    public async Task StartEngine_NotOwner_ReturnsForbid()
    {
        var result = await _controller.StartEngine(Guid.NewGuid());
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task StartEngine_HandlerSuccess_ReturnsOk()
    {
        var strategy = CreateStrategyWithPortfolio(Guid.NewGuid(), _traderId);
        _strategyRepo.Setup(r => r.GetByIdAsync(strategy.Id)).ReturnsAsync(strategy);
        _strategyRepo.Setup(r => r.GetByIdWithGraphAsync(strategy.Id)).ReturnsAsync(strategy);
        strategy.BotNodes.Add(new BotNode(strategy.Id, BotNodeType.Trigger, "T", """{"symbol":"BTC"}""", 0, 0));

        var result = await _controller.StartEngine(strategy.Id);

        var ok = Assert.IsType<OkObjectResult>(result);
        var msg = ok.Value.GetType().GetProperty("message")!.GetValue(ok.Value);
        Assert.Equal("Engine started.", msg);
    }

    [Fact]
    public async Task StartEngine_HandlerFails_ReturnsBadRequest()
    {
        var strategy = CreateStrategyWithPortfolio(Guid.NewGuid(), _traderId);
        _strategyRepo.Setup(r => r.GetByIdAsync(strategy.Id)).ReturnsAsync(strategy);
        _strategyRepo.Setup(r => r.GetByIdWithGraphAsync(strategy.Id)).ReturnsAsync(strategy);

        var result = await _controller.StartEngine(strategy.Id);

        var bad = Assert.IsType<BadRequestObjectResult>(result);
        var error = bad.Value.GetType().GetProperty("error")!.GetValue(bad.Value);
        Assert.Contains("Trigger", ((string)error!));
    }

    [Fact]
    public async Task StopEngine_NotOwner_ReturnsForbid()
    {
        var result = await _controller.StopEngine(Guid.NewGuid());
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task StopEngine_HandlerSuccess_ReturnsOk()
    {
        var strategy = CreateStrategyWithPortfolio(Guid.NewGuid(), _traderId);
        _strategyRepo.Setup(r => r.GetByIdAsync(strategy.Id)).ReturnsAsync(strategy);

        var result = await _controller.StopEngine(strategy.Id);

        var ok = Assert.IsType<OkObjectResult>(result);
        var msg = ok.Value.GetType().GetProperty("message")!.GetValue(ok.Value);
        Assert.Equal("Engine stopped.", msg);
    }

    [Fact]
    public async Task StopEngine_HandlerFails_ReturnsBadRequest()
    {
        var strategy = CreateStrategyWithPortfolio(Guid.NewGuid(), _traderId);
        _strategyRepo.Setup(r => r.GetByIdAsync(strategy.Id)).ReturnsAsync(strategy);
        _engine.Setup(e => e.StopStrategyAsync(It.IsAny<Guid>())).ThrowsAsync(new Exception("stop fail"));

        var result = await _controller.StopEngine(strategy.Id);

        var bad = Assert.IsType<BadRequestObjectResult>(result);
        var error = bad.Value.GetType().GetProperty("error")!.GetValue(bad.Value);
        Assert.Equal("stop fail", error);
    }

    [Fact]
    public async Task UpdateStrategyState_StrategyNotFound_ReturnsNotFound()
    {
        _strategyRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Strategy?)null);

        var result = await _controller.UpdateStrategyState(Guid.NewGuid(),
            new UpdateStrategyStateRequest { IsActive = true });

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task UpdateStrategyState_NotOwner_ReturnsForbid()
    {
        var strategy = CreateStrategyWithPortfolio(Guid.NewGuid(), "other-trader");
        _strategyRepo.Setup(r => r.GetByIdAsync(strategy.Id)).ReturnsAsync(strategy);

        var result = await _controller.UpdateStrategyState(strategy.Id,
            new UpdateStrategyStateRequest { IsActive = true });

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task UpdateStrategyState_Activate_ReturnsOk()
    {
        var strategyId = Guid.NewGuid();
        var strategy = CreateStrategyWithPortfolio(strategyId, _traderId);
        strategy.Deactivate();
        _strategyRepo.Setup(r => r.GetByIdAsync(strategyId)).ReturnsAsync(strategy);

        var result = await _controller.UpdateStrategyState(strategyId,
            new UpdateStrategyStateRequest { IsActive = true });

        var ok = Assert.IsType<OkObjectResult>(result);
        var msg = ok.Value.GetType().GetProperty("message")!.GetValue(ok.Value);
        Assert.Equal("Strategy activated successfully.", msg);
    }

    [Fact]
    public async Task UpdateStrategyState_Deactivate_ReturnsOk()
    {
        var strategyId = Guid.NewGuid();
        var strategy = CreateStrategyWithPortfolio(strategyId, _traderId);
        _strategyRepo.Setup(r => r.GetByIdAsync(strategyId)).ReturnsAsync(strategy);

        var result = await _controller.UpdateStrategyState(strategyId,
            new UpdateStrategyStateRequest { IsActive = false });

        var ok = Assert.IsType<OkObjectResult>(result);
        var msg = ok.Value.GetType().GetProperty("message")!.GetValue(ok.Value);
        Assert.Equal("Strategy deactivated successfully.", msg);
    }

    [Fact]
    public async Task AddNode_NoClaim_ReturnsForbid()
    {
        var controller = CreateControllerWithoutClaim();
        var result = await controller.AddNode(Guid.NewGuid(), new AddBotNodeRequest());
        Assert.IsType<ForbidResult>(result);
    }

    private StrategiesController CreateControllerWithoutClaim()
    {
        var ctrl = new StrategiesController(
            _strategyRepo.Object, _nodeRepo.Object, _edgeRepo.Object,
            new AddBotNodeCommandHandler(_nodeRepo.Object),
            new UpdateBotNodeCommandHandler(_nodeRepo.Object),
            new RemoveBotNodeCommandHandler(_nodeRepo.Object),
            new AddBotEdgeCommandHandler(_edgeRepo.Object, _nodeRepo.Object),
            new RemoveBotEdgeCommandHandler(_edgeRepo.Object),
            new StartEngineCommandHandler(_strategyRepo.Object, _engine.Object),
            new StopEngineCommandHandler(_engine.Object),
            new UpdateStrategyStateCommandHandler(_strategyRepo.Object));
        ctrl.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
        };
        return ctrl;
    }
}
