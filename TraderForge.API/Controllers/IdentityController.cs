using Microsoft.AspNetCore.Mvc;
using TraderForge.Application.DTOs;
using TraderForge.Application.Handlers;
using Microsoft.AspNetCore.Authorization;
using TraderForge.API.Mappers;
using TraderForge.API.Requests;

namespace TraderForge.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IdentityController : ControllerBase
{
    private readonly RegisterTraderCommandHandler _registerTraderCommandHandler;
    private readonly LoginTraderQueryHandler _loginTraderQueryHandler;
    private readonly RefreshTraderTokenQueryHandler _refreshTraderTokenQueryHandler;

    public IdentityController(
        RegisterTraderCommandHandler registerTraderCommandHandler, 
        LoginTraderQueryHandler loginTraderQueryHandler,
        RefreshTraderTokenQueryHandler refreshTraderTokenQueryHandler)
    {
        _registerTraderCommandHandler = registerTraderCommandHandler;
        _loginTraderQueryHandler = loginTraderQueryHandler;
        _refreshTraderTokenQueryHandler = refreshTraderTokenQueryHandler;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterTraderRequest request)
    {
        var command = request.ToCommand();
        var result = await _registerTraderCommandHandler.HandleAsync(command);

        if (result.IsSuccess)
        {
            return Ok(new { message = "Registration succesful! Enjoy your 7-Day free trial." });
        }

        return BadRequest(new { error = result.ErrorMessage });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginTraderRequest request)
    {
        var query = request.ToQuery();
        var result = await _loginTraderQueryHandler.HandleAsync(query);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return Unauthorized(new { error = result.ErrorMessage });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        var query = new TraderForge.Application.DTOs.Queries.RefreshTraderTokenQuery
        {
            AccessToken = request.AccessToken,
            RefreshToken = request.RefreshToken
        };

        var result = await _refreshTraderTokenQueryHandler.HandleAsync(query);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return Unauthorized(new { error = result.ErrorMessage });
    }



}
