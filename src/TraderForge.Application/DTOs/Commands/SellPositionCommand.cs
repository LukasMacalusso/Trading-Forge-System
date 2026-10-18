namespace TraderForge.Application.DTOs;

public class SellPositionCommand
{
    public Guid PositionId { get; set; }
    public decimal Quantity { get; set; }
}
