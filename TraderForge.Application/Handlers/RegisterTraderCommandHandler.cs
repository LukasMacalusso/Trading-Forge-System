using TraderForge.Application.Common;
using TraderForge.Application.DTOs;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Repositories;
using TraderForge.Domain.Services;

namespace TraderForge.Application.Handlers;

public class RegisterTraderCommandHandler
{
    private readonly IIdentityService _identityService;
    private readonly ITraderRepository _traderRepository;
    private readonly ISubscriptionPlanRepository _planRepository;

    public RegisterTraderCommandHandler(IIdentityService identityService, 
        ITraderRepository traderRepository, 
        ISubscriptionPlanRepository planRepository)
    {
        _identityService = identityService;
        _traderRepository = traderRepository;
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
        await _identityService.RegisterNewAccountAsync(newUserId,command.Email, command.Password);

        Trader newTrader = await GenerateTraderWithFreeTrialAsync(newUserId, command.Email);
        await _traderRepository.AddAsync(newTrader);
            
        return Result.Success();
    }

    private string GenerateNewAccountId()
    {
        return Guid.NewGuid().ToString();
    }

    private async Task<Trader> GenerateTraderWithFreeTrialAsync(string id, string email)
    {
        Trader newTrader = new Trader(id, email);
        newTrader.FreeTrialRegistrationDate = DateTime.UtcNow;
        newTrader.FreeTrialExpirationDate = DateTime.UtcNow.AddDays(7);
        newTrader.UserName = email;
        
        SubscriptionPlan basicPlan = await _planRepository.GetByNameAsync("basic");
        newTrader.AssignSubscriptionPlan(basicPlan);
        
        Portfolio newPortfolio = new Portfolio(id, basicPlan.InitialVirtualBalance);
        newTrader.Portfolios.Add(newPortfolio);
        return newTrader;
    }
    
    
    
}