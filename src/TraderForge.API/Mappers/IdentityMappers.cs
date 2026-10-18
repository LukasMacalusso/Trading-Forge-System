using TraderForge.API.Requests;
using TraderForge.Application.DTOs;

namespace TraderForge.API.Mappers;

public static class IdentityMappers
{
    public static RegisterTraderCommand ToCommand(this RegisterTraderRequest request)
    {
        return new RegisterTraderCommand()
        {
            Email = request.Email,
            Password = request.Password
        };
    }

    public static LoginTraderQuery ToQuery(this LoginTraderRequest request)
    {
        return new LoginTraderQuery()
        {
            Email = request.Email,
            Password = request.Password
        };
    }
}
