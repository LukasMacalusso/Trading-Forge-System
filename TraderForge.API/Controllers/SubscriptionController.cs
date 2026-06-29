using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TraderForge.API.Mappers;
using TraderForge.API.Requests;
using TraderForge.Application.DTOs;
using TraderForge.Application.Handlers;
using TraderForge.Domain.Services;

namespace TraderForge.API.Controllers;

[ApiController]
[Route("/api/[controller]")]
[Authorize(Roles = "Trader")]
public class SubscriptionController : ControllerBase
{
    private readonly ChangeSubscriptionCommandHandler _changeSubscriptionCommandHandler;
    private readonly CancelSubscriptionCommandHandler _cancelSubscriptionHandler;
    private readonly GetAllPlansQueryHandler _getAllPlansHandler;
    private readonly GetTraderPlanQueryHandler _getTraderSubscriptionPlanHandler;
    private readonly IDiscountService _discountService;

    public SubscriptionController(
        ChangeSubscriptionCommandHandler changeSubscriptionCommandHandler,
        CancelSubscriptionCommandHandler cancelSubscriptionHandler,
        IDiscountService discountService,
        GetAllPlansQueryHandler getAllPlansHandler,
        GetTraderPlanQueryHandler getTraderSubscriptionPlanHandler
        )
    {
        _changeSubscriptionCommandHandler = changeSubscriptionCommandHandler;
        _cancelSubscriptionHandler = cancelSubscriptionHandler;
        _discountService = discountService;
        _getAllPlansHandler = getAllPlansHandler;
        _getTraderSubscriptionPlanHandler = getTraderSubscriptionPlanHandler;
    }

    [HttpPost("pay")]
    public async Task<IActionResult> ProcessPayment([FromBody] ChangeSubscriptionRequest request)
    {
        var traderId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(traderId)) return Unauthorized(new { error = "Invalid token claims." });

        var command = request.ToCommand(traderId);

        var result = await _changeSubscriptionCommandHandler.HandleAsync(command);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.ErrorMessage });

        var discountOffer = await _discountService.GetEarlyCancellationOfferAsync(traderId, request.NewPlanId);

        return Ok(new
        {
            message = "Subscription successfully updated!",
            discount = discountOffer
        });
    }

    [HttpPost("cancel")]
    public async Task<IActionResult> CancelSubscription([FromQuery] bool forceCancel = false)
    {
        var traderId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(traderId)) return Unauthorized(new { error = "Invalid token claims." });

        var command = new CancelSubscriptionCommand { TraderId = traderId, ForceCancel = forceCancel };
        var result = await _cancelSubscriptionHandler.HandleAsync(command);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.ErrorMessage });

        if (!result.Value!.WasCancelled)
        {
            return Ok(new
            {
                message = "Wait! Do you still want to cancel?",
                discount = result.Value.RetentionOffer,
                requiresForceCancel = true
            });
        }

        return Ok(new
        {
            message = "Subscription successfully cancelled.",
            discount = result.Value.RetentionOffer
        });
    }


    [AllowAnonymous]
    [HttpGet("plans")]
    public async Task<IActionResult> GetSubscriptionPlans()
    {
        var result = await _getAllPlansHandler.HandleAsync(new GetAllPlansQuery());
        if (!result.IsSuccess)
            return BadRequest(new { error = result.ErrorMessage });
        return Ok(result.Value);
    }

    [HttpGet("trader-plan")]
    public async Task<IActionResult> GetTraderPlan()
    {
        var traderId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(traderId)) return Unauthorized(new { error = "Invalid token claims." });
        var result = await _getTraderSubscriptionPlanHandler.HandleAsync(new GetTraderPlanQuery { TraderId = traderId });
        if (!result.IsSuccess)
            return BadRequest(new { error = result.ErrorMessage });
        return Ok(new { plan = result.Value });
    }

}
