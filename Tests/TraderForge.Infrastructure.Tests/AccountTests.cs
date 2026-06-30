using TraderForge.Infrastructure;
using Xunit;

namespace TraderForge.Infrastructure.Tests;

public class AccountTests
{
    [Fact]
    public void Account_InheritsIdentityUser()
    {
        var account = new Account();
        Assert.IsAssignableFrom<Microsoft.AspNetCore.Identity.IdentityUser>(account);
    }

    [Fact]
    public void Account_RefreshToken_NullByDefault()
    {
        var account = new Account();
        Assert.Null(account.RefreshToken);
    }

    [Fact]
    public void Account_RefreshTokenExpiryTime_NullByDefault()
    {
        var account = new Account();
        Assert.Null(account.RefreshTokenExpiryTime);
    }

    [Fact]
    public void Account_CanSetRefreshToken()
    {
        var account = new Account();
        account.RefreshToken = "test-refresh-token";
        Assert.Equal("test-refresh-token", account.RefreshToken);
    }

    [Fact]
    public void Account_CanSetRefreshTokenExpiry()
    {
        var account = new Account();
        var expiry = DateTime.UtcNow.AddDays(1);
        account.RefreshTokenExpiryTime = expiry;
        Assert.Equal(expiry, account.RefreshTokenExpiryTime);
    }
}
