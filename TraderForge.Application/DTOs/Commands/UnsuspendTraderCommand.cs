namespace TraderForge.Application.DTOs.Commands;

public class UnsuspendTraderCommand
{
    public string TraderId { get; set; }

    public UnsuspendTraderCommand(string traderId)
    {
        TraderId = traderId;
    }
}