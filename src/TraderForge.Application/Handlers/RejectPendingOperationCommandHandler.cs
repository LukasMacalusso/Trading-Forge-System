using TraderForge.Application.DTOs;
using TraderForge.Domain.Common;
using TraderForge.Domain.Repositories;

namespace TraderForge.Application.Handlers;

public class RejectPendingOperationCommandHandler
{
    private readonly IPendingOperationRepository _pendingRepo;

    public RejectPendingOperationCommandHandler(IPendingOperationRepository pendingRepo)
    {
        _pendingRepo = pendingRepo;
    }

    public async Task<Result> HandleAsync(RejectPendingOperationCommand command)
    {
        var op = await _pendingRepo.GetByIdAsync(command.OperationId);
        if (op == null || op.IsResolved)
            return Result.Failure("Operation not found or already resolved.");

        if (op.Portfolio.TraderId != command.TraderId)
            return Result.Failure("Forbidden.");

        op.Resolve();
        _pendingRepo.Update(op);
        await _pendingRepo.SaveChangesAsync();

        return Result.Success();
    }
}
