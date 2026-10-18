using TraderForge.Domain.Common;
using TraderForge.Application.DTOs.Queries;
using TraderForge.Domain.Services;
using System.Threading.Tasks;
using System;

namespace TraderForge.Application.Handlers;

public class RefreshTraderTokenQueryHandler
{
    private readonly IIdentityService _identityService;

    public RefreshTraderTokenQueryHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<ResultGeneric<TokenResponse>> HandleAsync(RefreshTraderTokenQuery query)
    {
        try
        {
            var result = await _identityService.RefreshTokenAsync(query.AccessToken, query.RefreshToken);
            if (!result.IsSuccess)
            {
                return ResultGeneric<TokenResponse>.Failure(result.ErrorMessage ?? "Token refresh failed.");
            }
            return ResultGeneric<TokenResponse>.Success(result.Value!);
        }
        catch (Exception ex)
        {
            return ResultGeneric<TokenResponse>.Failure(ex.Message);
        }
    }
}
