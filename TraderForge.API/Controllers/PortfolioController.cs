using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TraderForge.API.Mappers;
using TraderForge.API.Requests;
using TraderForge.Application.DTOs;
using TraderForge.Application.Handlers;

namespace TraderForge.API.Controllers;

[ApiController]
[Route("/api/[controller]")]
[Authorize(Roles = "Trader")]
public class PortfolioController : ControllerBase
{
    private readonly GetActivePortfolioQueryHandler _getPortfolioHandler;
    private readonly GetStrategiesQueryHandler _getStrategiesHandler;
    private readonly GetPortfolioAssetsQueryHandler _getAssetsHandler;
    private readonly CreateStrategyCommandHandler _createStrategyHandler;
    private readonly UpdateStrategyStateCommandHandler _updateStrategyStateHandler;
    private readonly AddPortfolioAssetCommandHandler _addAssetHandler;
    private readonly RemovePortfolioAssetCommandHandler _removeAssetHandler;

    public PortfolioController(
        GetActivePortfolioQueryHandler getPortfolioHandler,
        GetStrategiesQueryHandler getStrategiesHandler,
        GetPortfolioAssetsQueryHandler getAssetsHandler,
        CreateStrategyCommandHandler createStrategyHandler,
        UpdateStrategyStateCommandHandler updateStrategyStateHandler,
        AddPortfolioAssetCommandHandler addAssetHandler,
        RemovePortfolioAssetCommandHandler removeAssetHandler)
    {
        _getPortfolioHandler = getPortfolioHandler;
        _getStrategiesHandler = getStrategiesHandler;
        _getAssetsHandler = getAssetsHandler;
        _createStrategyHandler = createStrategyHandler;
        _updateStrategyStateHandler = updateStrategyStateHandler;
        _addAssetHandler = addAssetHandler;
        _removeAssetHandler = removeAssetHandler;
    }

    [HttpGet]
    public async Task<IActionResult> GetActivePortfolio()
    {
        var traderId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(traderId))
            return Unauthorized(new { error = "Invalid token claims." });

        var result = await _getPortfolioHandler.HandleAsync(new GetActivePortfolioQuery { TraderId = traderId });
        if (!result.IsSuccess)
            return NotFound(new { error = result.ErrorMessage });

        return Ok(result.Value);
    }

    [HttpGet("strategies")]
    public async Task<IActionResult> GetStrategies()
    {
        var traderId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(traderId))
            return Unauthorized(new { error = "Invalid token claims." });

        var result = await _getStrategiesHandler.HandleAsync(new GetStrategiesQuery { TraderId = traderId });
        if (!result.IsSuccess)
            return BadRequest(new { error = result.ErrorMessage });

        return Ok(result.Value);
    }

    [HttpPost("strategies")]
    public async Task<IActionResult> CreateStrategy([FromBody] CreateStrategyRequest request)
    {
        var traderId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(traderId))
            return Unauthorized(new { error = "Invalid token claims." });

        var command = request.ToCommand(traderId);
        var result = await _createStrategyHandler.HandleAsync(command);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.ErrorMessage });

        return Ok(new { message = "Strategy created successfully." });
    }

    [HttpPut("strategies/{id:guid}/state")]
    public async Task<IActionResult> UpdateStrategyState(Guid id, [FromBody] UpdateStrategyStateRequest request)
    {
        var command = request.ToCommand(id);
        var result = await _updateStrategyStateHandler.HandleAsync(command);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.ErrorMessage });

        var state = command.IsActive ? "activated" : "deactivated";
        return Ok(new { message = $"Strategy {state} successfully." });
    }

    [HttpGet("assets")]
    public async Task<IActionResult> GetAssets()
    {
        var traderId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(traderId))
            return Unauthorized(new { error = "Invalid token claims." });

        var result = await _getAssetsHandler.HandleAsync(new GetPortfolioAssetsQuery { TraderId = traderId });
        if (!result.IsSuccess)
            return BadRequest(new { error = result.ErrorMessage });

        return Ok(result.Value);
    }

    [HttpPost("assets")]
    public async Task<IActionResult> AddAsset([FromBody] AddPortfolioAssetRequest request)
    {
        var traderId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(traderId))
            return Unauthorized(new { error = "Invalid token claims." });

        var command = request.ToCommand(traderId);
        var result = await _addAssetHandler.HandleAsync(command);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.ErrorMessage });

        return Ok(new { message = "Asset added successfully." });
    }

    [HttpDelete("assets/{id:guid}")]
    public async Task<IActionResult> RemoveAsset(Guid id)
    {
        var result = await _removeAssetHandler.HandleAsync(new RemovePortfolioAssetCommand { AssetId = id });

        if (!result.IsSuccess)
            return BadRequest(new { error = result.ErrorMessage });

        return Ok(new { message = "Asset removed successfully." });
    }
}
