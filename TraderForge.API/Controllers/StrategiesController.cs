using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TraderForge.API.Mappers;
using TraderForge.API.Requests;
using TraderForge.Application.DTOs;
using TraderForge.Application.Handlers;
using TraderForge.Domain.Repositories;

namespace TraderForge.API.Controllers;

[ApiController]
[Route("api/strategies")]
[Authorize(Roles = "Trader")]
public class StrategiesController : ControllerBase
{
    private readonly IStrategyRepository _strategyRepository;
    private readonly IBotNodeRepository _nodeRepository;
    private readonly IBotEdgeRepository _edgeRepository;
    private readonly AddBotNodeCommandHandler _addNodeHandler;
    private readonly UpdateBotNodeCommandHandler _updateNodeHandler;
    private readonly RemoveBotNodeCommandHandler _removeNodeHandler;
    private readonly AddBotEdgeCommandHandler _addEdgeHandler;
    private readonly RemoveBotEdgeCommandHandler _removeEdgeHandler;
    private readonly StartEngineCommandHandler _startEngineHandler;
    private readonly StopEngineCommandHandler _stopEngineHandler;
    private readonly UpdateStrategyStateCommandHandler _updateStrategyStateHandler;

    public StrategiesController(
        IStrategyRepository strategyRepository,
        IBotNodeRepository nodeRepository,
        IBotEdgeRepository edgeRepository,
        AddBotNodeCommandHandler addNodeHandler,
        UpdateBotNodeCommandHandler updateNodeHandler,
        RemoveBotNodeCommandHandler removeNodeHandler,
        AddBotEdgeCommandHandler addEdgeHandler,
        RemoveBotEdgeCommandHandler removeEdgeHandler,
        StartEngineCommandHandler startEngineHandler,
        StopEngineCommandHandler stopEngineHandler,
        UpdateStrategyStateCommandHandler updateStrategyStateHandler)
    {
        _strategyRepository = strategyRepository;
        _nodeRepository = nodeRepository;
        _edgeRepository = edgeRepository;
        _addNodeHandler = addNodeHandler;
        _updateNodeHandler = updateNodeHandler;
        _removeNodeHandler = removeNodeHandler;
        _addEdgeHandler = addEdgeHandler;
        _removeEdgeHandler = removeEdgeHandler;
        _startEngineHandler = startEngineHandler;
        _stopEngineHandler = stopEngineHandler;
        _updateStrategyStateHandler = updateStrategyStateHandler;
    }

    [HttpGet("{id:guid}/graph")]
    public async Task<IActionResult> GetGraph(Guid id)
    {
        var strategy = await _strategyRepository.GetByIdWithGraphAsync(id);
        if (strategy == null)
            return NotFound(new { error = "Strategy not found." });

        var traderId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (strategy.Portfolio.TraderId != traderId)
            return Forbid();

        return Ok(new
        {
            strategy.Id,
            strategy.Name,
            strategy.IsActive,
            strategy.IsEngineActive,
            Nodes = strategy.BotNodes.Select(n => new
            {
                n.Id, n.Type, n.Name, n.Config, n.PositionX, n.PositionY, n.IsActive
            }),
            Edges = strategy.BotEdges.Select(e => new
            {
                e.Id, e.SourceNodeId, e.SourcePort, e.TargetNodeId
            })
        });
    }

    [HttpPost("{id:guid}/nodes")]
    public async Task<IActionResult> AddNode(Guid id, [FromBody] AddBotNodeRequest request)
    {
        if (!await OwnsStrategyAsync(id))
            return Forbid();

        var command = request.ToCommand(id);
        var result = await _addNodeHandler.HandleAsync(command);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.ErrorMessage });

        return Ok(new { nodeId = result.Value });
    }

    [HttpPut("{strategyId:guid}/nodes/{nodeId:guid}")]
    public async Task<IActionResult> UpdateNode(Guid strategyId, Guid nodeId, [FromBody] UpdateBotNodeRequest request)
    {
        if (!await OwnsStrategyAsync(strategyId))
            return Forbid();

        var command = request.ToCommand(nodeId);
        var result = await _updateNodeHandler.HandleAsync(command);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.ErrorMessage });

        return Ok(new { message = "Node updated." });
    }

    [HttpDelete("{strategyId:guid}/nodes/{nodeId:guid}")]
    public async Task<IActionResult> RemoveNode(Guid strategyId, Guid nodeId)
    {
        if (!await OwnsStrategyAsync(strategyId))
            return Forbid();

        var result = await _removeNodeHandler.HandleAsync(new RemoveBotNodeCommand { Id = nodeId, StrategyId = strategyId });

        if (!result.IsSuccess)
            return BadRequest(new { error = result.ErrorMessage });

        return Ok(new { message = "Node removed." });
    }

    [HttpPost("{id:guid}/edges")]
    public async Task<IActionResult> AddEdge(Guid id, [FromBody] AddBotEdgeRequest request)
    {
        if (!await OwnsStrategyAsync(id))
            return Forbid();

        var command = request.ToCommand(id);
        var result = await _addEdgeHandler.HandleAsync(command);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.ErrorMessage });

        return Ok(new { edgeId = result.Value });
    }

    [HttpDelete("{strategyId:guid}/edges/{edgeId:guid}")]
    public async Task<IActionResult> RemoveEdge(Guid strategyId, Guid edgeId)
    {
        if (!await OwnsStrategyAsync(strategyId))
            return Forbid();

        var result = await _removeEdgeHandler.HandleAsync(new RemoveBotEdgeCommand { Id = edgeId, StrategyId = strategyId });

        if (!result.IsSuccess)
            return BadRequest(new { error = result.ErrorMessage });

        return Ok(new { message = "Edge removed." });
    }

    [HttpPost("{id:guid}/engine/start")]
    public async Task<IActionResult> StartEngine(Guid id)
    {
        if (!await OwnsStrategyAsync(id))
            return Forbid();

        var result = await _startEngineHandler.HandleAsync(new StartEngineCommand { StrategyId = id });

        if (!result.IsSuccess)
            return BadRequest(new { error = result.ErrorMessage });

        return Ok(new { message = "Engine started." });
    }

    [HttpPost("{id:guid}/engine/stop")]
    public async Task<IActionResult> StopEngine(Guid id)
    {
        if (!await OwnsStrategyAsync(id))
            return Forbid();

        var result = await _stopEngineHandler.HandleAsync(new StopEngineCommand { StrategyId = id });

        if (!result.IsSuccess)
            return BadRequest(new { error = result.ErrorMessage });

        return Ok(new { message = "Engine stopped." });
    }

    [HttpPut("{id:guid}/state")]
    public async Task<IActionResult> UpdateStrategyState(Guid id, [FromBody] UpdateStrategyStateRequest request)
    {
        var strategy = await _strategyRepository.GetByIdAsync(id);
        if (strategy == null)
            return NotFound(new { error = "Strategy not found." });

        var traderId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (strategy.Portfolio.TraderId != traderId)
            return Forbid();

        var command = request.ToCommand(id);
        var result = await _updateStrategyStateHandler.HandleAsync(command);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.ErrorMessage });

        var state = command.IsActive ? "activated" : "deactivated";
        return Ok(new { message = $"Strategy {state} successfully." });
    }

    private async Task<bool> OwnsStrategyAsync(Guid strategyId)
    {
        var traderId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(traderId)) return false;

        var strategy = await _strategyRepository.GetByIdAsync(strategyId);
        return strategy != null && strategy.Portfolio.TraderId == traderId;
    }
}
