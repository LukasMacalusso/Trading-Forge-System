namespace TraderForge.Application.DTOs.Queries;

public class RefreshTraderTokenQuery
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
}
