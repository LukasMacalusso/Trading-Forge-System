using TraderForge.Application.DTOs;
using TraderForge.Domain.Common;
using TraderForge.Domain.Repositories;
using TraderForge.Domain.Services;

namespace TraderForge.Application.Handlers;

public class ApprovePendingOperationCommandHandler
{
    private readonly IPendingOperationRepository _pendingRepo;
    private readonly ICommissionService _commissionService;

    public ApprovePendingOperationCommandHandler(
        IPendingOperationRepository pendingRepo,
        ICommissionService commissionService)
    {
        _pendingRepo = pendingRepo;
        _commissionService = commissionService;
    }

    public async Task<Result> HandleAsync(ApprovePendingOperationCommand command)
    {
        var op = await _pendingRepo.GetByIdAsync(command.OperationId);
        if (op == null || op.IsResolved)
            return Result.Failure("Operation not found or already resolved.");

        if (op.Portfolio.TraderId != command.TraderId)
            return Result.Failure("Forbidden.");

        if (DateTime.UtcNow > op.ExpiresAt)
        {
            op.Resolve();
            await _pendingRepo.SaveChangesAsync();
            return Result.Failure("Operation expired.");
        }

        if (op.Action == "buy" || op.Action == "Buy")
            op.Portfolio.BuyPosition(op.Symbol, op.Quantity, op.CurrentPrice, _commissionService);
        else
            op.Portfolio.SellPosition(op.Symbol, op.Quantity, op.CurrentPrice, _commissionService);

        op.Resolve();
        _pendingRepo.Update(op);
        await _pendingRepo.SaveChangesAsync();

        return Result.Success();
    }
}
