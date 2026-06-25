using TraderForge.Domain.Enums;
using TraderForge.Domain.Services;

namespace TraderForge.Domain.Entities;

public class Portfolio
{
    public Guid Id { get; private set; }
    public decimal VirtualBalance { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ClosedAt { get; private set; }


    public ICollection<Strategy> Strategies { get; private set; } = new List<Strategy>();
    public ICollection<Position> Positions { get; private set; } = new List<Position>();
    public ICollection<Transaction> Transactions { get; private set; } = new List<Transaction>();
    public ICollection<Order> Orders { get; private set; } = new List<Order>();

    public string TraderId { get; private set; }
    public Trader Trader { get; private set; } = null!;

    private Portfolio() { }

    public Portfolio(string traderId, decimal initialBalance)
    {
        Id = Guid.NewGuid();
        TraderId = traderId;
        VirtualBalance = initialBalance;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void FreezeSimulation()
    {
        IsActive = false;
        ClosedAt = DateTime.UtcNow;
    }

    public void DeductFunds(decimal total, string type, string? symbol, decimal? qty, decimal? price, decimal commission)
    {
        var balanceBefore = VirtualBalance;
        VirtualBalance -= total;

        Transactions.Add(new Transaction(
            Id, type, total, balanceBefore, VirtualBalance, commission, symbol, qty, price));
            
        EvaluateBankruptcyStatus();
    }

    public void AddFunds(decimal total, string type, string? symbol, decimal? qty, decimal? price, decimal commission)
    {
        var balanceBefore = VirtualBalance;
        VirtualBalance += total;

        Transactions.Add(new Transaction(
            Id, type, total, balanceBefore, VirtualBalance, commission, symbol, qty, price));
    }

    public void BuyPosition(string symbol, decimal quantity, decimal price, ICommissionService commissionService)
    {
        var subtotal = quantity * price;
        var commission = commissionService.Calculate(subtotal);
        var totalCost = subtotal + commission;

        if (totalCost > VirtualBalance)
            throw new InvalidOperationException($"Insufficient balance. Required: ${totalCost:F2}, Available: ${VirtualBalance:F2}");

        UpsertPosition(symbol, quantity, price);

        DeductFunds(totalCost, "Buy", symbol, quantity, price, commission);
        EvaluateBankruptcyStatus();

        var order = new Order(Id, symbol, OrderSide.Buy, OrderType.Market, quantity, price, commission, totalCost, OrderStatus.Filled);
        Orders.Add(order);
    }

    private void UpsertPosition(string symbol, decimal quantity, decimal price)
    {
        var existingPosition = Positions.FirstOrDefault(p => p.Symbol == symbol);
        if (existingPosition != null)
        {
            existingPosition.Update(quantity, price);
        }
        else
        {
            Positions.Add(new Position(Guid.NewGuid(), symbol, quantity, price, Id));
        }
    }

    public void SellPosition(string symbol, decimal quantity, decimal price, ICommissionService commissionService)
    {
        var existingPosition = Positions.FirstOrDefault(p => p.Symbol == symbol);
        if (existingPosition == null)
            throw new InvalidOperationException("Position not found.");

        if (quantity > existingPosition.Quantity)
            throw new InvalidOperationException($"Cannot sell {quantity}. You only own {existingPosition.Quantity}.");

        var proceeds = quantity * price;
        var commission = commissionService.Calculate(proceeds);
        var netProceeds = proceeds - commission;

        if (quantity >= existingPosition.Quantity)
        {
            Positions.Remove(existingPosition);
        }
        else
        {
            existingPosition.ReduceQuantity(quantity);
        }

        AddFunds(netProceeds, "Sell", symbol, quantity, price, commission);

        var order = new Order(Id, symbol, OrderSide.Sell, OrderType.Market, quantity, price, commission, netProceeds, OrderStatus.Filled);
        Orders.Add(order);
    }

    private void EvaluateBankruptcyStatus()
    {
        if (VirtualBalance <= 0)
            FreezeSimulation();
    }
}
