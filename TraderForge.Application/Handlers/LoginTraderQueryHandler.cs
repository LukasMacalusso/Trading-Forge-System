using TraderForge.Domain.Common;
using TraderForge.Application.DTOs;
using TraderForge.Domain.Repositories;
using TraderForge.Domain.Services;
namespace TraderForge.Application.Handlers;

public class LoginTraderQueryHandler
{
    private readonly IIdentityService _identityService;

    public LoginTraderQueryHandler(IIdentityService identityService, ITraderRepository traderRepository)
    {
        _identityService = identityService;
    }


    public async Task<ResultGeneric<TokenResponse>> HandleAsync(LoginTraderQuery query)
    {
        try
        {
            return await RequestTokenFromIdentity(query);
        }
        catch (Exception ex)
        {
            return ResultGeneric<TokenResponse>.Failure(ex.Message);
        }
    }

    private async Task<ResultGeneric<TokenResponse>> RequestTokenFromIdentity(LoginTraderQuery query)
    {
        var result = await _identityService.GetValidatedTokenAsync(query.Email, query.Password);
        if (!result.IsSuccess)
        {
            return ResultGeneric<TokenResponse>.Failure(result.ErrorMessage ?? "Login failed.");
        }
        return ResultGeneric<TokenResponse>.Success(result.Value!);
    }

}
