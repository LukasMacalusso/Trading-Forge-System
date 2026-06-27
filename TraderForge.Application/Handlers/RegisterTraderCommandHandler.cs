using TraderForge.Domain.Common;
using TraderForge.Application.DTOs;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Interfaces;
using TraderForge.Domain.Repositories;
using TraderForge.Domain.Services;

namespace TraderForge.Application.Handlers;

public class RegisterTraderCommandHandler
{
    private readonly IIdentityService _identityService;
    private readonly ITraderRepository _traderRepository;
    private readonly ITraderFactory _traderFactory;
    private readonly ISubscriptionPlanRepository _planRepository;

    public RegisterTraderCommandHandler(
        IIdentityService identityService, 
        ITraderRepository traderRepository, 
        ITraderFactory traderFactory,
        ISubscriptionPlanRepository planRepository) 
    {
        _identityService = identityService;
        _traderRepository = traderRepository;
        _traderFactory = traderFactory;
        _planRepository = planRepository;
    }

    public async Task<Result> HandleAsync(RegisterTraderCommand command)
    {
        try
        {
            return await ExecuteRegistration(command);
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }
    }

    private async Task<Result> ExecuteRegistration(RegisterTraderCommand command)
    {
        string newUserId = GenerateNewAccountId();
        
        var identityResult = await _identityService.RegisterNewAccountAsync(newUserId, command.Email, command.Password);
        if (!identityResult.IsSuccess)
        {
            return Result.Failure(identityResult.ErrorMessage ?? "Registration failed.");
        }

        Trader newTrader = _traderFactory.CreateWithFreeTrial(newUserId, command.Email);
        
        SubscriptionPlan basicPlan = await _planRepository.GetByNameAsync("basic");
        newTrader.InitializeWithPlan(basicPlan);
        
        await _traderRepository.AddAsync(newTrader);
        return Result.Success();
    }
    private static string GenerateNewAccountId()
    {
        return Guid.NewGuid().ToString();
    }
}