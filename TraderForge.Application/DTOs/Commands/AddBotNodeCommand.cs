using TraderForge.Domain.Enums;

namespace TraderForge.Application.DTOs;

public class AddBotNodeCommand
{
    public Guid StrategyId { get; set; }
    public BotNodeType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Config { get; set; } = "{}";
    public double PositionX { get; set; }
    public double PositionY { get; set; }
}
