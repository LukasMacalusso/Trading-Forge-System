namespace TraderForge.Application.DTOs;

public class UpdateBotNodeCommand
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Config { get; set; } = "{}";
    public double PositionX { get; set; }
    public double PositionY { get; set; }
}
