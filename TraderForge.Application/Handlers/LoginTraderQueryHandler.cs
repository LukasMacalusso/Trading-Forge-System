using TraderForge.Domain.Common;
using TraderForge.Application.DTOs;
using TraderForge.Domain.Repositories;
using TraderForge.Domain.Services;
namespace TraderForge.Application.Handlers;

public class LoginTraderQueryHandler
{
    private readonly IIdentityService _identityService;
    private readonly ITraderRepository _traderRepository;

    public LoginTraderQueryHandler(IIdentityService identityService, ITraderRepository traderRepository) {
        _identityService = identityService;
        _traderRepository = traderRepository;
    }


    public async Task<ResultGeneric<string>> HandleAsync(LoginTraderQuery query)
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
        var result = await _identityService.GetValidatedTokenAsync(query.Email, query.Password);
        if (!result.IsSuccess)
        {
            return ResultGeneric<string>.Failure(result.ErrorMessage ?? "Login failed.");
        }
        return ResultGeneric<string>.Success(result.Value!);
    }

}