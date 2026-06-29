namespace TraderForge.Application.DTOs;

public class CancelSubscriptionCommand
{
    public string TraderId { get; set; } = null!;
    public bool ForceCancel { get; set; }
}