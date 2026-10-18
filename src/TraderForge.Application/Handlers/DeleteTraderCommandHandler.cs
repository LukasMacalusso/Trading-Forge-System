using TraderForge.Domain.Common;
using TraderForge.Application.DTOs;
using TraderForge.Domain.Repositories;
using TraderForge.Domain.Services;
using System;
using System.Threading.Tasks;

namespace TraderForge.Application.Handlers;

public class DeleteTraderCommandHandler
{
    private readonly ITraderRepository _traderRepository;
    private readonly IIdentityService _identityService;

    public DeleteTraderCommandHandler(ITraderRepository traderRepository, IIdentityService identityService)
    {
        _traderRepository = traderRepository;
        _identityService = identityService;
    }

    public async Task<Result> HandleAsync(DeleteTraderCommand command)
    {
        try
        {
            var trader = await _traderRepository.GetByIdAsync(command.TraderId);
            if (trader == null)
            {
                return Result.Failure("Trader not found.");
            }

            var identityResult = await _identityService.DeleteAccountAsync(command.TraderId);
            if (!identityResult.IsSuccess)
            {
                return Result.Failure(identityResult.ErrorMessage ?? "Failed to delete identity account.");
            }

            await _traderRepository.DeleteAsync(trader);
            await _traderRepository.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
