using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TraderForge.API.Mappers;
using TraderForge.API.Requests;
using TraderForge.Application.DTOs;
using TraderForge.Application.Handlers;

namespace TraderForge.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Trader")]
public class PortfolioController : ControllerBase
{
    private readonly GetActivePortfolioQueryHandler _getPortfolioHandler;
    private readonly GetPortfolioHistoryQueryHandler _getPortfolioHistoryHandler;
    private readonly GetStrategiesQueryHandler _getStrategiesHandler;
    private readonly GetPositionsQueryHandler _getPositionsHandler;
    private readonly CreateStrategyCommandHandler _createStrategyHandler;
    private readonly BuyPositionCommandHandler _buyPositionHandler;
    private readonly SellPositionCommandHandler _sellPositionHandler;
    private readonly GetTransactionsQueryHandler _getTransactionsHandler;
    private readonly GetOrdersQueryHandler _getOrdersHandler;
    private readonly ResetSimulationCommandHandler _resetSimulationHandler;

    public PortfolioController(
        GetActivePortfolioQueryHandler getPortfolioHandler,
        GetPortfolioHistoryQueryHandler getPortfolioHistoryHandler,
        GetStrategiesQueryHandler getStrategiesHandler,
        GetPositionsQueryHandler getPositionsHandler,
        CreateStrategyCommandHandler createStrategyHandler,
        BuyPositionCommandHandler buyPositionHandler,
        SellPositionCommandHandler sellPositionHandler,
        GetTransactionsQueryHandler getTransactionsHandler,
        GetOrdersQueryHandler getOrdersHandler,
        ResetSimulationCommandHandler resetSimulationHandler)
    {
        _getPortfolioHandler = getPortfolioHandler;
        _getPortfolioHistoryHandler = getPortfolioHistoryHandler;
        _getStrategiesHandler = getStrategiesHandler;
        _getPositionsHandler = getPositionsHandler;
        _createStrategyHandler = createStrategyHandler;
        _buyPositionHandler = buyPositionHandler;
        _sellPositionHandler = sellPositionHandler;
        _getTransactionsHandler = getTransactionsHandler;
        _getOrdersHandler = getOrdersHandler;
        _resetSimulationHandler = resetSimulationHandler;
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
    public async Task<IActionResult> GetStrategies([FromQuery] Guid? portfolioId)
    {
        var traderId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(traderId))
            return Unauthorized(new { error = "Invalid token claims." });

        var result = await _getStrategiesHandler.HandleAsync(new GetStrategiesQuery { TraderId = traderId, PortfolioId = portfolioId });
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

        return Ok(new { id = result.Value, message = "Strategy created successfully." });
    }

    [HttpGet("positions")]
    public async Task<IActionResult> GetPositions([FromQuery] Guid? portfolioId)
    {
        var traderId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(traderId))
            return Unauthorized(new { error = "Invalid token claims." });

        var result = await _getPositionsHandler.HandleAsync(new GetPositionsQuery { TraderId = traderId, PortfolioId = portfolioId });
        if (!result.IsSuccess)
            return BadRequest(new { error = result.ErrorMessage });

        return Ok(result.Value);
    }

    [HttpPost("positions/buy")]
    public async Task<IActionResult> BuyPosition([FromBody] BuyPositionRequest request)
    {
        var traderId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(traderId))
            return Unauthorized(new { error = "Invalid token claims." });

        var command = request.ToCommand(traderId);
        var result = await _buyPositionHandler.HandleAsync(command);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.ErrorMessage });

        return Ok(new { message = "Position purchased successfully." });
    }

    [HttpPost("positions/{id:guid}/sell")]
    public async Task<IActionResult> SellPosition(Guid id, [FromBody] SellPositionRequest request)
    {
        var command = request.ToCommand(id);
        var result = await _sellPositionHandler.HandleAsync(command);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.ErrorMessage });

        return Ok(new { message = "Position sold successfully." });
    }

    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions([FromQuery] Guid? portfolioId)
    {
        var traderId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(traderId))
            return Unauthorized(new { error = "Invalid token claims." });

        var result = await _getTransactionsHandler.HandleAsync(new GetTransactionsQuery { TraderId = traderId, PortfolioId = portfolioId });
        if (!result.IsSuccess)
            return BadRequest(new { error = result.ErrorMessage });

        return Ok(result.Value);
    }

    [HttpGet("orders")]
    public async Task<IActionResult> GetOrders([FromQuery] Guid? portfolioId)
    {
        var traderId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(traderId))
            return Unauthorized(new { error = "Invalid token claims." });

        var result = await _getOrdersHandler.HandleAsync(new GetOrdersQuery { TraderId = traderId, PortfolioId = portfolioId });
        if (!result.IsSuccess)
            return BadRequest(new { error = result.ErrorMessage });

        return Ok(result.Value);
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetPortfolioHistory()
    {
        var traderId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(traderId))
            return Unauthorized(new { error = "Invalid token claims." });

        var result = await _getPortfolioHistoryHandler.HandleAsync(new GetPortfolioHistoryQuery { TraderId = traderId });
        if (!result.IsSuccess)
            return BadRequest(new { error = result.ErrorMessage });

        return Ok(result.Value);
    }

    [HttpPost("reset")]
    public async Task<IActionResult> ResetSimulation()
    {
        var traderId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(traderId))
            return Unauthorized(new { error = "Invalid token claims." });

        var command = new ResetSimulationCommand { TraderId = traderId };
        var result = await _resetSimulationHandler.HandleAsync(command);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.ErrorMessage });

        return Ok(new { message = "Simulation reset successfully." });
    }
}
