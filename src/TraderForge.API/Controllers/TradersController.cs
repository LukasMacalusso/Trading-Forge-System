using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TraderForge.Application.DTOs;
using TraderForge.Application.Handlers;

namespace TraderForge.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Trader")]
public class TradersController : ControllerBase
{
    private readonly DeleteTraderCommandHandler _deleteTraderCommandHandler;

    public TradersController(DeleteTraderCommandHandler deleteTraderCommandHandler)
    {
        _deleteTraderCommandHandler = deleteTraderCommandHandler;
    }

    [HttpDelete("me")]
    public async Task<IActionResult> DeleteAccount()
    {
        var traderId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(traderId))
            return Unauthorized(new { error = "Invalid token claims." });

        var command = new DeleteTraderCommand { TraderId = traderId };
        var result = await _deleteTraderCommandHandler.HandleAsync(command);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.ErrorMessage });

        return Ok(new { message = "Account successfully deleted." });
    }
}
