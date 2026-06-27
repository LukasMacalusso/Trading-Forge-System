using Microsoft.AspNetCore.Mvc;
using TraderForge.API.Mappers;
using TraderForge.API.Requests;
using TraderForge.Application.Handlers;

namespace TraderForge.API.Controllers;

[ApiController]
[Route("api/prices")]
public class PricesController : ControllerBase
{
    private readonly GetMarketPricesQueryHandler _handler;

    public PricesController(GetMarketPricesQueryHandler handler)
    {
        _handler = handler;
    }

    [HttpPost]
    public async Task<IActionResult> GetPrices([FromBody] GetMarketPricesRequest request) 
    {
        if (request.Symbols == null || request.Symbols.Count == 0)
        {
            return BadRequest("You must provide at least one symbol.");
        }

        var query = request.ToQuery();
        var result = await _handler.HandleAsync(query); 
        return Ok(result.Value); 
    }
    
    [HttpGet("historical")]
    public async Task<IActionResult> GetHistoricalCandles(
        [FromServices] GetHistoricalCandlesQueryHandler historicalHandler,
        [FromQuery] string symbol, 
        [FromQuery] string interval = "1h", 
        [FromQuery] int limit = 500) 
    {
        if (string.IsNullOrWhiteSpace(symbol)) return BadRequest("Symbol is required.");
        
        var query = new TraderForge.Application.DTOs.Queries.GetHistoricalCandlesQuery(symbol, interval, limit);
        var result = await historicalHandler.HandleAsync(query);
        
        if (!result.IsSuccess) return BadRequest(result.ErrorMessage);
        return Ok(result.Value);
    }
}