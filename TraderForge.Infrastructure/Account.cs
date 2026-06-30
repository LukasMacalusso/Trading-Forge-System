using Microsoft.AspNetCore.Identity;

namespace TraderForge.Infrastructure;

public class Account : IdentityUser
{
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
}
