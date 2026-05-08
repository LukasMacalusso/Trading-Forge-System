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
        var result = await _handler.GetMarketPricesAsync(query); 
        return Ok(result.Value); 
    }
}