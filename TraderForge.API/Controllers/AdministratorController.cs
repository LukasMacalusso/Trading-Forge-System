using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TraderForge.API.Mappers;
using TraderForge.API.Requests;
using TraderForge.Application.DTOs;
using TraderForge.Application.Handlers;
using TraderForge.Domain.Interfaces;

namespace TraderForge.API.Controllers;

[ApiController]
[Route("/api/admin/plans")]
[Authorize(Roles = "SystemAdmin")]
public class AdministratorController : ControllerBase
{
    private readonly GetAllPlansQueryHandler _getAllPlansHandler;
    private readonly CreatePlanCommandHandler _createPlanHandler;
    private readonly UpdatePlanCommandHandler _updatePlanHandler;
    private readonly DeletePlanCommandHandler _deletePlanHandler;
    private readonly ISubscriptionPlanRepository _planRepository;

    public AdministratorController(
        GetAllPlansQueryHandler getAllPlansHandler,
        CreatePlanCommandHandler createPlanHandler,
        UpdatePlanCommandHandler updatePlanHandler,
        DeletePlanCommandHandler deletePlanHandler,
        ISubscriptionPlanRepository planRepository)
    {
        _getAllPlansHandler = getAllPlansHandler;
        _createPlanHandler = createPlanHandler;
        _updatePlanHandler = updatePlanHandler;
        _deletePlanHandler = deletePlanHandler;
        _planRepository = planRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _getAllPlansHandler.GetAllSubscriptionPlans(new GetAllPlansQuery());
        if (!result.IsSuccess)
            return BadRequest(new { error = result.ErrorMessage });
        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var plan = await _planRepository.GetByIdAsync(id);
        if (plan is null)
            return NotFound(new { error = "Subscription plan not found." });
        return Ok(plan);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePlanRequest request)
    {
        var command = request.ToCommand();
        var result = await _createPlanHandler.HandleAsync(command);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.ErrorMessage });
        return Ok(new { message = "Plan created successfully." });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePlanRequest request)
    {
        var command = request.ToCommand(id);
        var result = await _updatePlanHandler.HandleAsync(command);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.ErrorMessage });
        return Ok(new { message = "Plan updated successfully." });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _deletePlanHandler.HandleAsync(id);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.ErrorMessage });
        return Ok(new { message = "Plan deleted successfully." });
    }
}
