namespace TraderForge.Application.DTOs;

public class RegisterTraderCommand
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}