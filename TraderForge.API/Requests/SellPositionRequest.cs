using TraderForge.Application.DTOs;

namespace TraderForge.API.Requests;

public class SellPositionRequest
{
    public decimal Quantity { get; set; }

    public SellPositionCommand ToCommand(Guid positionId) => new()
    {
        PositionId = positionId,
        Quantity = Quantity
    };
}
