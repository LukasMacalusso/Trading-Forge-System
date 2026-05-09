using TraderForge.Application.Common;
using TraderForge.Application.DTOs;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Interfaces;

namespace TraderForge.Application.Handlers;

public class LoginTraderQueryHandler
{
    
    private readonly IIdentityService _identityService;
    private readonly ITraderRepository _traderRepository;

    public LoginTraderQueryHandler(IIdentityService identityService, ITraderRepository traderRepository)
    {
        _identityService = identityService;
        _traderRepository = traderRepository;
    }


    public async Task<ResultGeneric<string>> GetLoginTokenAsync(LoginTraderQuery query)
    {
        try
        {
            return await RequestTokenFromIdentity(query);
        }
        catch (Exception ex)
        {
            return ResultGeneric<string>.Failure(ex.Message);
        }
    }

    private async Task<ResultGeneric<string>> RequestTokenFromIdentity(LoginTraderQuery query)
    {
        string jwtToken = await _identityService.GetValidatedTokenAsync(query.Email, query.Password);
        return ResultGeneric<string>.Success(jwtToken);
    }

}