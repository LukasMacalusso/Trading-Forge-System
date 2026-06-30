using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using TraderForge.Domain.Common;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Repositories;
using TraderForge.Infrastructure.Services;

namespace TraderForge.Infrastructure.Tests;

public class IdentityServiceTests
{
    private const string JwtSecret = "test-secret-key-that-is-at-least-32-characters-long!";
    private const string JwtIssuer = "TestIssuer";
    private const string JwtAudience = "TestAudience";

    private readonly Mock<IUserStore<Account>> _userStoreMock;
    private readonly Mock<IPasswordHasher<Account>> _passwordHasher;
    private readonly UserManager<Account> _userManager;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<ITraderRepository> _traderRepoMock;
    private readonly IdentityService _service;

    public IdentityServiceTests()
    {
        _userStoreMock = new Mock<IUserStore<Account>>();
        _userStoreMock.As<IUserPasswordStore<Account>>();
        _userStoreMock.As<IUserEmailStore<Account>>();
        _userStoreMock.As<IUserClaimStore<Account>>();
        _passwordHasher = new Mock<IPasswordHasher<Account>>();
        _passwordHasher
            .Setup(x => x.HashPassword(It.IsAny<Account>(), It.IsAny<string>()))
            .Returns("hashed-password");
        _userManager = new UserManager<Account>(
            _userStoreMock.Object, null!, _passwordHasher.Object, null!, null!, null!, null!, null!, null!);
        _configurationMock = new Mock<IConfiguration>();
        _traderRepoMock = new Mock<ITraderRepository>();
        SetupJwtConfiguration();
        _service = new IdentityService(_userManager, _configurationMock.Object, _traderRepoMock.Object);
    }

    private void SetupJwtConfiguration()
    {
        _configurationMock.Setup(x => x["JwtSettings:Issuer"]).Returns(JwtIssuer);
        _configurationMock.Setup(x => x["JwtSettings:Audience"]).Returns(JwtAudience);
        _configurationMock.Setup(x => x["JwtSettings:Secret"]).Returns(JwtSecret);
    }

    private static string GenerateTestJwtToken(string userId, string email, bool expired = true)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(ClaimTypes.Role, "Trader")
        };
        var token = new JwtSecurityToken(
            issuer: JwtIssuer,
            audience: JwtAudience,
            claims: claims,
            expires: expired ? DateTime.UtcNow.AddMinutes(-5) : DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [Fact]
    public async Task RegisterNewAccountAsync_Success_ReturnsSuccess()
    {
        _userStoreMock
            .Setup(x => x.CreateAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _service.RegisterNewAccountAsync("user1", "test@test.com", "Pass123!");

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task RegisterNewAccountAsync_Failure_ReturnsError()
    {
        var identityError = new IdentityError { Description = "Password too weak" };
        _userStoreMock
            .Setup(x => x.CreateAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdentityResult.Failed(identityError));

        var result = await _service.RegisterNewAccountAsync("user1", "test@test.com", "weak");

        Assert.False(result.IsSuccess);
        Assert.Contains("Password too weak", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteAccountAsync_UserFound_ReturnsSuccess()
    {
        var account = new Account { Id = "user1", Email = "test@test.com" };
        _userStoreMock
            .Setup(x => x.FindByIdAsync("user1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);
        _userStoreMock
            .Setup(x => x.DeleteAsync(account, It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _service.DeleteAccountAsync("user1");

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task DeleteAccountAsync_UserNotFound_ReturnsFailure()
    {
        _userStoreMock
            .Setup(x => x.FindByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Account?)null);

        var result = await _service.DeleteAccountAsync("nonexistent");

        Assert.False(result.IsSuccess);
        Assert.Contains("Account not found", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteAccountAsync_DeleteFails_ReturnsError()
    {
        var account = new Account { Id = "user1", Email = "test@test.com" };
        _userStoreMock
            .Setup(x => x.FindByIdAsync("user1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);
        _userStoreMock
            .Setup(x => x.DeleteAsync(account, It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Delete failed" }));

        var result = await _service.DeleteAccountAsync("user1");

        Assert.False(result.IsSuccess);
        Assert.Contains("Delete failed", result.ErrorMessage);
    }

    [Fact]
    public async Task GetValidatedTokenAsync_InvalidCredentials_ReturnsFailure()
    {
        _userStoreMock.As<IUserEmailStore<Account>>()
            .Setup(x => x.FindByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Account?)null);

        var result = await _service.GetValidatedTokenAsync("test@test.com", "wrong");

        Assert.False(result.IsSuccess);
        Assert.Contains("Invalid Credentials", result.ErrorMessage);
    }

    [Fact]
    public async Task GetValidatedTokenAsync_SuspendedAccount_ReturnsFailure()
    {
        var account = new Account { Id = "user1", Email = "test@test.com" };
        var trader = new Trader("user1", "test@test.com");
        trader.Suspend("Violated terms");
        _userStoreMock.As<IUserEmailStore<Account>>()
            .Setup(x => x.FindByEmailAsync("test@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);
        _userStoreMock.As<IUserPasswordStore<Account>>()
            .Setup(x => x.GetPasswordHashAsync(account, It.IsAny<CancellationToken>()))
            .ReturnsAsync("hashed-password");
        _passwordHasher
            .Setup(x => x.VerifyHashedPassword(account, "hashed-password", "Pass123!"))
            .Returns(PasswordVerificationResult.Success);
        _traderRepoMock.Setup(x => x.GetByIdAsync("user1")).ReturnsAsync(trader);

        var result = await _service.GetValidatedTokenAsync("test@test.com", "Pass123!");

        Assert.False(result.IsSuccess);
        Assert.Contains("Violated terms", result.ErrorMessage);
    }

    [Fact]
    public async Task GetValidatedTokenAsync_Success_ReturnsTokenResponse()
    {
        var account = new Account { Id = "user1", Email = "test@test.com" };
        var trader = new Trader("user1", "test@test.com");
        _userStoreMock.As<IUserEmailStore<Account>>()
            .Setup(x => x.FindByEmailAsync("test@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);
        _userStoreMock.As<IUserPasswordStore<Account>>()
            .Setup(x => x.GetPasswordHashAsync(account, It.IsAny<CancellationToken>()))
            .ReturnsAsync("hashed-password");
        _passwordHasher
            .Setup(x => x.VerifyHashedPassword(account, "hashed-password", "Pass123!"))
            .Returns(PasswordVerificationResult.Success);
        _traderRepoMock.Setup(x => x.GetByIdAsync("user1")).ReturnsAsync(trader);
        _userStoreMock.As<IUserClaimStore<Account>>()
            .Setup(x => x.GetClaimsAsync(account, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Claim>());
        _userStoreMock
            .Setup(x => x.UpdateAsync(account, It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _service.GetValidatedTokenAsync("test@test.com", "Pass123!");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEmpty(result.Value.AccessToken);
        Assert.NotEmpty(result.Value.RefreshToken);
    }

    [Fact]
    public async Task RefreshTokenAsync_InvalidToken_ReturnsFailure()
    {
        var result = await _service.RefreshTokenAsync("invalid-token", "refresh");

        Assert.False(result.IsSuccess);
        Assert.Contains("Invalid access token or refresh token", result.ErrorMessage);
    }

    [Fact]
    public async Task RefreshTokenAsync_Success_ReturnsTokenResponse()
    {
        var account = new Account
        {
            Id = "user1",
            Email = "test@test.com",
            RefreshToken = "valid-refresh-token",
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1)
        };
        var token = GenerateTestJwtToken("user1", "test@test.com", expired: true);
        _userStoreMock
            .Setup(x => x.FindByIdAsync("user1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);
        _userStoreMock.As<IUserClaimStore<Account>>()
            .Setup(x => x.GetClaimsAsync(account, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Claim>());
        _userStoreMock
            .Setup(x => x.UpdateAsync(account, It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _service.RefreshTokenAsync(token, "valid-refresh-token");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEmpty(result.Value.AccessToken);
        Assert.NotEmpty(result.Value.RefreshToken);
    }

    [Fact]
    public async Task RefreshTokenAsync_ExpiredRefreshToken_ReturnsFailure()
    {
        var account = new Account
        {
            Id = "user1",
            Email = "test@test.com",
            RefreshToken = "stale-refresh-token",
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(-1)
        };
        var token = GenerateTestJwtToken("user1", "test@test.com", expired: true);
        _userStoreMock
            .Setup(x => x.FindByIdAsync("user1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var result = await _service.RefreshTokenAsync(token, "stale-refresh-token");

        Assert.False(result.IsSuccess);
        Assert.Contains("Invalid access token or refresh token", result.ErrorMessage);
    }
}
