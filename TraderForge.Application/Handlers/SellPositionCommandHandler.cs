using TraderForge.Application.Common;
using TraderForge.Application.DTOs;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Repositories;
using TraderForge.Domain.Services;

namespace TraderForge.Application.Handlers;

public class SellPositionCommandHandler
{
    private readonly IPositionRepository _positionRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly ICommissionService _commissionService;
    private readonly IMarketService _marketService;

    public SellPositionCommandHandler(
        IPositionRepository positionRepository,
        IOrderRepository orderRepository,
        ICommissionService commissionService,
        IMarketService marketService)
    {
        _positionRepository = positionRepository;
        _orderRepository = orderRepository;
        _commissionService = commissionService;
        _marketService = marketService;
    }

    public async Task<Result> HandleAsync(SellPositionCommand command)
    {
        try
        {
            return await ExecuteSellAsync(command);
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }
    }

    private async Task<Result> ExecuteSellAsync(SellPositionCommand command)
    {
        var position = await _positionRepository.GetByIdWithPortfolioAsync(command.PositionId);
        if (position == null)
            return Result.Failure("Position not found.");

        if (!IsMarketOpen(position.Symbol))
            return Result.Failure($"The market for {position.Symbol} is currently closed.");

        if (!HasSufficientQuantity(command.Quantity, position.Quantity))
            return Result.Failure($"Cannot sell {command.Quantity}. You only own {position.Quantity}.");

        var (netProceeds, commission) = ProcessFunds(position, command.Quantity);
        await UpdateOrRemovePositionAsync(position, command.Quantity);
        await CreateOrderReceiptAsync(position.PortfolioId, position.Symbol, command.Quantity, position.EntryPrice, commission, netProceeds);

        await _positionRepository.SaveChangesAsync();
        await _orderRepository.SaveChangesAsync();

        return Result.Success();
    }

    private bool IsMarketOpen(string symbol)
    {
        return _marketService.IsMarketOpen(symbol);
    }

    private bool HasSufficientQuantity(decimal sellQuantity, decimal availableQuantity)
    {
        return sellQuantity <= availableQuantity;
    }

    private (decimal NetProceeds, decimal Commission) ProcessFunds(Position position, decimal sellQuantity)
    {
        var proceeds = CalculateProceeds(sellQuantity, position.EntryPrice);
        var commission = _commissionService.Calculate(proceeds);
        var netProceeds = proceeds - commission;

        position.Portfolio.AddFunds(netProceeds, "Sell", position.Symbol, sellQuantity, position.EntryPrice, commission);
        return (netProceeds, commission);
    }

    private decimal CalculateProceeds(decimal quantity, decimal price)
    {
        return quantity * price;
    }

    private async Task UpdateOrRemovePositionAsync(Position position, decimal sellQuantity)
    {
        if (sellQuantity >= position.Quantity)
        {
            await _positionRepository.RemoveAsync(position);
        }
        else
        {
            var remainingQuantity = position.Quantity - sellQuantity;
            position.Update(remainingQuantity, position.EntryPrice);
        }
    }

    private async Task CreateOrderReceiptAsync(Guid portfolioId, string symbol, decimal quantity, decimal price, decimal commission, decimal netProceeds)
    {
        var order = new Order(
            portfolioId, symbol, "Sell", "Market", quantity, price, commission, netProceeds, "Filled");
        await _orderRepository.AddAsync(order);
    }
}
